"""
Copyright (C) 2019 NVIDIA Corporation.  All rights reserved.
Licensed under the CC BY-NC-SA 4.0 license (https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode).
"""

from data.base_dataset import BaseDataset, get_params, get_transform
from PIL import Image
import util.util as util
import os


class Pix2pixDataset(BaseDataset):
    @staticmethod
    def modify_commandline_options(parser, is_train):
        # parser.add_argument('--no_pairing_check', action='store_true',
                            # help='If specified, skip sanity check of correct label-image file pairing')
        return parser

    def initialize(self, opt):
        self.opt = opt
        label_paths = self.get_paths(opt) #TODO modify
        util.natural_sort(label_paths)
        self.label_paths = label_paths[:opt.max_dataset_size] #TODO modify
        self.dataset_size = len(self.label_paths)

    def get_paths(self, opt):
        label_paths = []
        assert False, "A subclass of Pix2pixDataset must override self.get_paths(self, opt)"
        return label_paths

    def __getitem__(self, index):
        # Label Image
        label_path = self.label_paths[index]
        label = Image.open(label_path)
        params = get_params(self.opt, label.size)
        transform_label = get_transform(
            self.opt, params, method=Image.NEAREST, normalize=False)
        label_tensor = transform_label(label) * 255.0

        # 'unknown' is opt.label_nc
        label_tensor[label_tensor == 255] = self.opt.label_nc

        input_dict = {'label': label_tensor}

        # Give subclasses a chance to modify the final output
        self.postprocess(input_dict)

        return input_dict

    def postprocess(self, input_dict):
        return input_dict

    def __len__(self):
        return self.dataset_size
