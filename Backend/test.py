"""
Copyright (C) 2019 NVIDIA Corporation.  All rights reserved.
Licensed under the CC BY-NC-SA 4.0 license (https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode).
"""

import importlib
import os
from collections import OrderedDict

import torch

import data
from data.base_dataset import BaseDataset
from models.pix2pix_model import Pix2PixModel
from options.test_options import TestOptions
from util.visualizer import Visualizer


def run(verbose=False):
    opt = TestOptions().parse(verbose)

    dataset_name = "coco"
    dataset_filename = "data." + dataset_name + "_dataset"

    datasetlib = importlib.import_module(dataset_filename)

    dataset = None
    target_dataset_name = dataset_name.replace('_', '') + 'dataset'

    for name, cls in datasetlib.__dict__.items():
        if name.lower() == target_dataset_name.lower() \
                and issubclass(cls, BaseDataset):
            dataset = cls

    # opt.use_vae = True
    opt.crop_size = 256

    opt.display_winsize = 1024
    opt.load_size = 256
    opt.preprocess_mode='resize_and_crop'
    instance = dataset()
    instance.initialize(opt)
    print(opt.load_size)
    print(opt)
    if verbose:
        print("dataset [%s] of size %d was created" %
          (type(instance).__name__, len(instance)))

    dataloader = torch.utils.data.DataLoader(
        instance,
        batch_size=opt.batchSize,
        shuffle=not opt.serial_batches,
        num_workers=int(opt.nThreads),
        drop_last=opt.isTrain
    )
    model = Pix2PixModel(opt, verbose)
    model.eval()
    visualizer = Visualizer(opt)
    if verbose:
        print(dataloader)
    for i, data_i in enumerate(dataloader):

        if i * opt.batchSize >= opt.how_many:
            break

        # this is just a dictionary that contains tensors and stuff?
        generated = model(data_i, mode='inference', verbose=verbose)
        
        for b in range(generated.shape[0]):
            # Should only be one?
            image_dir = os.path.join(
                os.path.dirname(__file__),
                "img"
            )
            return visualizer.save_images(generated[b], image_dir, verbose=verbose)


if __name__ == "__main__":
    run()
