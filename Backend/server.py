from flask import Flask, render_template, request
from werkzeug.utils import secure_filename


import atexit
import os
import cv2
import uuid

TEMP_FILE_FOLDER = "temp/"
app = Flask(__name__)

@app.route('/uploader', methods=['GET', 'POST'])
def upload_file():
    if request.method == 'POST':
        f = request.files['file']
        postfix = f.filename.split(".")[-1]
        file_name = TEMP_FILE_FOLDER + str(uuid.uuid4()) + "." + postfix
        f.save(file_name)
        print(file_name)
        img = cv2.imread(file_name)
        if img is None:
            print("Could not read the image.")
        else:
            print("wtf then:/")
            cv2.imshow("Display window", img)
            cv2.waitKey(0)
        return 'file uploaded successfully'


if __name__ == '__main__':
    app.run(debug=True)


def exit_handler():
    dir = TEMP_FILE_FOLDER
    for f in os.listdir(dir):
        os.remove(os.path.join(dir, f))

atexit.register(exit_handler)