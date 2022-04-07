"""
Copyright (C) 2019 NVIDIA Corporation.  All rights reserved.
Licensed under the CC BY-NC-SA 4.0 license (https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode).
"""

import torch
import models.networks as networks
import util.util as util
import warnings


class Pix2PixModel(torch.nn.Module):
    @staticmethod
    def modify_commandline_options(parser, is_train):
        networks.modify_commandline_options(parser, is_train)
        return parser

    def __init__(self, opt,verbose=True):
        super().__init__()

        self.opt = opt
        self.FloatTensor = torch.cuda.FloatTensor if self.use_gpu() \
            else torch.FloatTensor
        self.ByteTensor = torch.cuda.ByteTensor if self.use_gpu() \
            else torch.ByteTensor


        self.netG = self.initialize_networks(opt,verbose)


    # Entry point for all calls involving forward pass
    # of deep networks. We used this approach since DataParallel module
    # can't parallelize custom functions, we branch to different
    # routines based on |mode|.
    def forward(self, data, mode, verbose=True):
        """Foward pass, overrides something"""
        input_semantics = self.preprocess_input(data, verbose)
        with torch.no_grad():
            return self.generate_fake(input_semantics)

    def initialize_networks(self, opt, verbose=True):
        # IDK
        netG = util.load_network(networks.define_G(opt,verbose), 'G', opt.which_epoch, opt)
        return netG

    # preprocess the input, such as moving the tensors to GPUs and
    # transforming the label map to one-hot encoding
    # |data|: dictionary of the input data

    def preprocess_input(self, data,verbose=True):
        """Magic stuff happens"""
        # move to GPU and change data types
        data['label'] = data['label'].long()
        if self.use_gpu():
            data['label'] = data['label'].cuda()

        # create one-hot label map
        label_map = data['label']
        if verbose:
            print(label_map.shape)

        label_map = label_map.clamp(0, self.opt.label_nc)

        bs, _, h, w = label_map.size()
        num_classes = 184
        input_label = self.FloatTensor(bs, num_classes, h, w).zero_()
        input_semantics = input_label.scatter_(1, label_map, 1.0)
        return input_semantics

    def generate_fake(self, input_semantics):
        """Generates the fake image from the input semantics"""
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            fake_image = self.netG(input_semantics, z=None)

        return fake_image

    # Given fake and real image, return the prediction of discriminator
    # for each fake and real image.

    def get_edges(self, t):
        """What arcane magic is this?!"""
        edge = self.ByteTensor(t.size()).zero_()
        edge[:, :, :, 1:] = edge[:, :, :, 1:] | (
            t[:, :, :, 1:] != t[:, :, :, :-1])
        edge[:, :, :, :-1] = edge[:, :, :, :-
                                  1] | (t[:, :, :, 1:] != t[:, :, :, :-1])
        edge[:, :, 1:, :] = edge[:, :, 1:, :] | (
            t[:, :, 1:, :] != t[:, :, :-1, :])
        edge[:, :, :-1, :] = edge[:, :, :-1,
                                  :] | (t[:, :, 1:, :] != t[:, :, :-1, :])
        return edge.float()

    def use_gpu(self):
        return len(self.opt.gpu_ids) > 0
