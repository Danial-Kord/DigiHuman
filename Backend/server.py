import time

from flask import Flask, render_template, request
from werkzeug.utils import secure_filename


import atexit
import os
import cv2
import uuid
import mimetypes
import pose_estimator
# import mocap
import mediaPipeFace
from flask import jsonify , stream_with_context
from flask import Response
from threading import Thread
import subprocess
from color_grey_conversion import color_to_grey
from test import run
from flask import send_file


mimetypes.init()

TEMP_FILE_FOLDER = "temp/"
STATIC_IMG_FOLDER = "results"

pose_video_data = {} # name of file to array of json
pose_video_data_statues = {} # name of file to array of json

hand_pose_video_data = {} # name of file to array of json
hand_pose_video_data_statues = {} # name of file to array of json

full_pose_video_data = {} # name of file to array of json
full_pose_video_data_statues = {} # name of file to array of json

face_pose_video_data = {} # name of file to array of json
face_pose_video_data_statues = {} # name of file to array of json

process_reqs = []

# GAU gan values
IMG_FOLDER = os.path.join(os.path.dirname(__file__), "dataset/val_img")
INST_FOLDER = os.path.join(os.path.dirname(__file__), "dataset/val_inst")
LABEL_FOLDER = os.path.join(os.path.dirname(__file__), "dataset/val_label")
verbose = True

def copy_file(old, new):
    command_string = "cp " + old + " " + new
    subprocess.check_output(command_string.split(" "))

def make_processable(greyscale_fname, output_color_file):
    # Inst folder
    ouptut_greyscale_file = INST_FOLDER + "/" + greyscale_fname
    # Converts the file to greyscale and saves it to the inst folder?
    if verbose:
        print(output_color_file, ouptut_greyscale_file)
    color_to_grey.convert_rgb_image_to_greyscale(
        output_color_file,
        ouptut_greyscale_file
    )

    ouptut_greyscale_file_labels = LABEL_FOLDER + "/" + greyscale_fname

    copy_file(ouptut_greyscale_file, ouptut_greyscale_file_labels)

    ouptut_greyscale_file_img = IMG_FOLDER + "/" + greyscale_fname
    copy_file(ouptut_greyscale_file, ouptut_greyscale_file_img)

def parse_static_filepath(filepath):
    split_filepath = filepath.split('/')
    while len(split_filepath) > 2:
        split_filepath.pop(0)

    return '/'.join(split_filepath)

def run_model(filename):
    """Runs the pretrained COCO model"""
    # TODO check to see if this goes any faster with GPUS enabled...
    # TODO make is it so that concurrent users won't mess with eachother :P aka have hashed or something dataset routes...
    # that will also take a lot of cleaning up...
    # TODO figure out how to not do this from the command line...
    return run(verbose=verbose)

def GauGanRunner(output_color_file):
    greyscale_fname = "greyscale.png"

    make_processable(greyscale_fname, output_color_file)

    # We shouldnt need to pass it a string anymore
    export_image_location = run_model(greyscale_fname)
    if verbose:
        print(export_image_location)
    static_image_location = parse_static_filepath(export_image_location)
    if verbose:
        print(static_image_location)
    return  static_image_location




#for round robin process
def process_queue():
    while True:
        time.sleep(0.2)
        # print("wtf")
        global process_reqs
        if len(process_reqs) != 0:
            file_name = str(process_reqs.pop(0))
            # # thread = Thread(target=calculate_video_pose_estimation,args=file_name)
            # print("process {} started".format(file_name))
            # thread.start()
            # thread.join()
            # print("process {} finished".format(file_name))
            print("process {} started".format(file_name))
            calculate_video_pose_estimation(file_name)
            print("process {} finished".format(file_name))



app = Flask(__name__)


# filename = path to video, json_array_len= how many frames data should be sent per request
def calculate_video_pose_estimation(file_name):
    print()
    global pose_video_data
    global pose_video_data_statues

    json_data = []
    # print("wtf")
    for i in pose_estimator.Pose_Video(file_name):
        pose_video_data[file_name].append(i)
    pose_video_data_statues[file_name] = True #means process is finished
    # return pose_estimator.Pose_Video(file_name)

# filename = path to video, json_array_len= how many frames data should be sent per request
def calculate_video_hand_pose_estimation(file_name):
    print()
    global hand_pose_video_data
    global hand_pose_video_data_statues

    json_data = []
    # print("wtf")
    for i in pose_estimator.Hands_Full(file_name):
        hand_pose_video_data[file_name].append(i)
    hand_pose_video_data_statues[file_name] = True #means process is finished
    # return pose_estimator.Pose_Video(file_name)

# filename = path to video, json_array_len= how many frames data should be sent per request
def calculate_video_full_pose_estimation(file_name):
    print()
    global full_pose_video_data
    global full_pose_video_data_statues

    json_data = []
    # print("wtf")
    for i in pose_estimator.Complete_pose_Video(file_name):
        full_pose_video_data[file_name].append(i)
    full_pose_video_data_statues[file_name] = True #means process is finished
    # return pose_estimator.Pose_Video(file_name)





def calculate_video_mocap_estimation(file_name):
    global face_pose_video_data
    global face_pose_video_data_statues

    for i in mediaPipeFace.Calculate_Face_Mocap(file_name):
        face_pose_video_data[file_name].append(i)
    face_pose_video_data_statues[file_name] = True #means process is finished








@app.route("/hand", methods=["POST"])
def get_frame_hand_pose():

    global hand_pose_video_data
    global hand_pose_video_data_statues


    request_json = request.get_json()  # Get request body (JSON type)
    index = request_json['index']
    file_name = str(request_json['fileName'])
    req = request.data
    try:
        if hand_pose_video_data.keys().__contains__(file_name) is False:
            print("Wrong!")
            return Response("Wrong input!")
        while True:
            if len(hand_pose_video_data[file_name]) >= index + 1:
                # print(hand_pose_video_data[file_name][index])
                return jsonify(hand_pose_video_data[file_name][index])
            elif hand_pose_video_data_statues[file_name] is False:
                time.sleep(0.15)
            else:
                return Response("Done")
    except:
        return Response("Good luck!")



@app.route("/holistc", methods=["POST"])
def get_frame_full_pose():

    global full_pose_video_data
    global full_pose_video_data_statues


    request_json = request.get_json()  # Get request body (JSON type)
    index = request_json['index']
    file_name = str(request_json['fileName'])
    req = request.data
    try:
        if full_pose_video_data.keys().__contains__(file_name) is False:
            print("Wrong!")
            return Response("Wrong input!")
        while True:
            if len(full_pose_video_data[file_name]) >= index + 1:
                # print(hand_pose_video_data[file_name][index])
                return jsonify(full_pose_video_data[file_name][index])
            elif full_pose_video_data_statues[file_name] is False:
                time.sleep(0.15)
            else:
                return Response("Done")
    except:
        return Response("Good luck!")



#TODO better ending connection "Done!"
@app.route("/face", methods=["POST"])  # Hard-coded login route
def get_frame_facial_expression():
    global face_pose_video_data
    global face_pose_video_data_statues
    request_json = request.get_json()  # Get request body (JSON type)
    index = request_json['index']
    file_name = str(request_json['fileName'])
    req = request.data
    try:
        if face_pose_video_data.keys().__contains__(file_name) is False:
            print("Wrong!")
            return Response("Wrong input!")
        while True:
            if len(face_pose_video_data[file_name]) >= index + 1:
                return jsonify(face_pose_video_data[file_name][index])
            elif face_pose_video_data_statues[file_name] is False:
                time.sleep(0.15)
            else:
                return Response("Done")
    except:
        return Response("Good luck!")



@app.route("/pose", methods=["POST"])  # Hard-coded login route
def get_frame_pose():

    global pose_video_data
    global pose_video_data_statues


    request_json = request.get_json()  # Get request body (JSON type)
    index = request_json['index']
    file_name = str(request_json['fileName'])
    req = request.data
    #print(file_name)
    try:
        if pose_video_data.keys().__contains__(file_name) is False:
            print("Wrong!")
            return Response("Wrong input!")
        while True:
            if len(pose_video_data[file_name]) >= index + 1:
                # print(pose_video_data[file_name][index])
                return jsonify(pose_video_data[file_name][index])
            elif pose_video_data_statues[file_name] is False:
                time.sleep(0.15)
            else:
                return Response("Done")
    except:
        return Response("Good luck!")

# processing received file
@app.route('/uploader', methods=['GET', 'POST'])
def upload_file():

    if request.method == 'POST':
        f = request.files['file']
        postfix = f.filename.split(".")[-1]
        file_name = TEMP_FILE_FOLDER + str(uuid.uuid4()) + "." + postfix
        f.save(file_name)

        # checking file type
        mimestart = mimetypes.guess_type(file_name)[0]
        if mimestart != None:
            mimestart = mimestart.split('/')[0]
            if mimestart in ['video']:
                # global process_reqs
                # process_reqs.append(file_name)
                print(":(")
                global pose_video_data
                global pose_video_data_statues
                pose_video_data[file_name] = []
                pose_video_data_statues[file_name] = False
                thread2 = Thread(target=calculate_video_pose_estimation,args=(file_name,))
                thread2.start()
                print("video type")
                cap = cv2.VideoCapture(file_name)
                tframe = cap.get(cv2.CAP_PROP_FRAME_COUNT)  # get total frame count
                width = cap.get(cv2.CAP_PROP_FRAME_WIDTH)  # float `width`
                height = cap.get(cv2.CAP_PROP_FRAME_HEIGHT)  # float `height`
                aspectRatio = width / height
                cap.release()

                res = {
                    'file' : file_name,
                    'totalFrames': int(tframe),
                    'aspectRatio': aspectRatio
                }
                return jsonify(res)

            elif mimestart in ['image']:
                print("image type")
                GuGanImage = GauGanRunner(file_name)
                return send_file(GuGanImage, mimetype='image/png')
            else:
                print("Wrong input!")
                return "Oops!"
        # print(file_name)
        # img = cv2.imread(file_name)
        # if img is None:
        #     print("Could not read the image.")
        # else:
        #     print("wtf then:/")
        #     cv2.imshow("Display window", img)
        #     cv2.waitKey(0)
        return 'file uploaded successfully'


# processing received file
@app.route('/handUploader', methods=['GET', 'POST'])
def upload_hand_video():
    if request.method == 'POST':
        f = request.files['file']
        postfix = f.filename.split(".")[-1]
        file_name = TEMP_FILE_FOLDER + str(uuid.uuid4()) + "." + postfix
        f.save(file_name)
        # checking file type
        mimestart = mimetypes.guess_type(file_name)[0]
        if mimestart != None:
            mimestart = mimestart.split('/')[0]
            if mimestart in ['video']:
                global hand_pose_video_data
                global hand_pose_video_data_statues
                hand_pose_video_data[file_name] = []
                hand_pose_video_data_statues[file_name] = False
                thread2 = Thread(target=calculate_video_hand_pose_estimation,args=(file_name,))
                thread2.start()
                cap = cv2.VideoCapture(file_name)
                tframe = cap.get(cv2.CAP_PROP_FRAME_COUNT)  # get total frame count
                width = cap.get(cv2.CAP_PROP_FRAME_WIDTH)  # float `width`
                height = cap.get(cv2.CAP_PROP_FRAME_HEIGHT)  # float `height`
                aspectRatio = width / height
                cap.release()
                res = {
                    'file' : file_name,
                    'totalFrames': int(tframe),
                    'aspectRatio': aspectRatio
                }
                return jsonify(res)
            else:
                print("Wrong input!")
                return "Oops!"
        return 'file uploaded successfully'



# processing received file
@app.route('/holisticUploader', methods=['GET', 'POST'])
def upload_holistic_video():

    if request.method == 'POST':
        f = request.files['file']
        postfix = f.filename.split(".")[-1]
        file_name = TEMP_FILE_FOLDER + str(uuid.uuid4()) + "." + postfix
        f.save(file_name)

        # checking file type
        mimestart = mimetypes.guess_type(file_name)[0]
        if mimestart != None:
            mimestart = mimestart.split('/')[0]
            if mimestart in ['video']:
                global hand_pose_video_data
                global hand_pose_video_data_statues
                full_pose_video_data[file_name] = []
                full_pose_video_data_statues[file_name] = False
                thread2 = Thread(target=calculate_video_full_pose_estimation,args=(file_name,))
                thread2.start()
                print("video type")
                cap = cv2.VideoCapture(file_name)
                tframe = cap.get(cv2.CAP_PROP_FRAME_COUNT)  # get total frame count
                width = cap.get(cv2.CAP_PROP_FRAME_WIDTH)  # float `width`
                height = cap.get(cv2.CAP_PROP_FRAME_HEIGHT)  # float `height`
                aspectRatio = width / height
                cap.release()

                res = {
                    'file' : file_name,
                    'totalFrames': int(tframe),
                    'aspectRatio': aspectRatio
                }
                return jsonify(res)
            else:
                print("Wrong input!")
                return "Oops!"
        return 'file uploaded successfully'



@app.route('/faceUploader', methods=['GET', 'POST'])
def upload_face_video():

    if request.method == 'POST':
        f = request.files['file']
        postfix = f.filename.split(".")[-1]
        file_name = TEMP_FILE_FOLDER + str(uuid.uuid4()) + "." + postfix
        f.save(file_name)

        # checking file type
        mimestart = mimetypes.guess_type(file_name)[0]
        if mimestart != None:
            mimestart = mimestart.split('/')[0]
            if mimestart in ['video']:
                # global process_reqs
                # process_reqs.append(file_name)
                global face_pose_video_data
                global face_pose_video_data_statues
                face_pose_video_data[file_name] = []
                face_pose_video_data_statues[file_name] = False
                thread = Thread(target=calculate_video_mocap_estimation,args=(file_name,))
                thread.start()
                print("video type")
                cap = cv2.VideoCapture(file_name)
                tframe = cap.get(cv2.CAP_PROP_FRAME_COUNT)  # get total frame count
                width = cap.get(cv2.CAP_PROP_FRAME_WIDTH)  # float `width`
                height = cap.get(cv2.CAP_PROP_FRAME_HEIGHT)  # float `height`
                aspectRatio = width / height
                cap.release()

                res = {
                    'file' : file_name,
                    'totalFrames': int(tframe),
                    'aspectRatio': aspectRatio
                }
                return jsonify(res)

            else:
                print("Wrong input!")
                return "Oops!"
        return 'file uploaded successfully'




def run_server():
    app.run(debug=True)












def exit_handler():
    dir = TEMP_FILE_FOLDER
    for f in os.listdir(dir):
        os.remove(os.path.join(dir, f))


if __name__ == '__main__':
    isExist = os.path.exists(TEMP_FILE_FOLDER)

    if not isExist:
        # Create a new directory because it does not exist
        os.makedirs(TEMP_FILE_FOLDER)
    atexit.register(exit_handler)
    run_server()
    # app.run(debug=True)
    # thread1 = Thread(target=run_server())
    # thread2 = Thread(target=process_queue())
    # thread2.start()
    # thread1.start()
