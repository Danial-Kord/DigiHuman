from PIL import Image
import json

# sub 1 to actual value


def convert_rgb_image_to_greyscale(input_file, output_file):
    label_to_grey = {}
    label_to_rgb = {}
    rgb_to_label = {}

    with open("labels.txt", "r") as labels:
        for line in labels.readlines():
            line = line.strip()
            split_line = line.split(' ')
            if len(split_line) < 3:
                continue
            grey, label, rgb_list = split_line
            rgb = tuple(map(int, rgb_list.split(',')))

            label_to_rgb[label] = rgb
            rgb_to_label[rgb] = label
            # I forget why we are off by 1
            label_to_grey[label] = int(grey) - 1

    in_img = Image.open(input_file)
    out_img = Image.new("L", (in_img.size[0], in_img.size[1]))
    pixels = in_img.load()
    p_o = out_img.load()
    grey = (0)

    for i in range(in_img.size[0]):    # for every col:
        for j in range(in_img.size[1]):    # For every row
            # print(pixels[i, j][0:3])
            if(pixels[i, j][0:3] in rgb_to_label.keys()):
                label = rgb_to_label[pixels[i, j][0:3]]
                grey = label_to_grey[label]
            p_o[i, j] = grey
    out_img.save(output_file)


def main():
    in_file = "masterpiece.png"
    out_file = "b.png"
    convert_rgb_image_to_greyscale(in_file, out_file)


if __name__ == '__main__':
    main()
