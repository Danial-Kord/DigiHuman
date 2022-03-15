import time

from flask import Flask, render_template, request
from werkzeug.utils import secure_filename


import atexit
import os
import cv2
import uuid
import mimetypes
import pose_estimator
from flask import jsonify , stream_with_context
from flask import Response
from threading import Thread

mimetypes.init()

TEMP_FILE_FOLDER = "temp/"


pose_video_data = {} # name of file to array of json
pose_video_data_statues = {} # name of file to array of json
process_reqs = []

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



@app.route("/pose", methods=["POST"])  # Hard-coded login route
def get_frame_pose():

    global pose_video_data
    global pose_video_data_statues


    request_json = request.get_json()  # Get request body (JSON type)
    index = request_json['index']
    file_name = str(request_json['fileName'])
    req = request.data
    print(file_name)
    try:
        if pose_video_data.keys().__contains__(file_name) is False:
            print("Wrong!")
            return Response("Wrong input!")
        while True:
            if len(pose_video_data[file_name]) >= index + 1:
                print(pose_video_data[file_name][index])
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
                return Response(file_name)
                # return jsonify(calculate_video_pose_estimation(file_name))
                # return Response(stream_with_context(calculate_video_pose_estimation(file_name)),mimetype="text/json")
            elif mimestart in ['image']:
                print("image type")
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


def run_server():
    app.run(debug=True)












def exit_handler():
    dir = TEMP_FILE_FOLDER
    for f in os.listdir(dir):
        os.remove(os.path.join(dir, f))


if __name__ == '__main__':
    atexit.register(exit_handler)
    run_server()
    # app.run(debug=True)
    # thread1 = Thread(target=run_server())
    # thread2 = Thread(target=process_queue())
    # thread2.start()
    # thread1.start()
