import os

image_files = []
os.chdir(os.path.join("example_photo"))
for filename in os.listdir(os.getcwd()):
    if filename.endswith(".jpg"):
        image_files.append(filename)
os.chdir("..")
with open("train.txt", "w") as outfile:
    for image in image_files:
        outfile.write("data/project05/example_photo/" + image)
        outfile.write("\n")
    outfile.close()
os.chdir("..")