###
#
#   Koromo Copy - File enumerator
#
#   Copyright (C) 2018. dc-koromo. All rights reserved.
#
###

import re
import sys
from os import walk

path = " ".join(sys.argv[1:])

print("working on ... " + path)

pattern = ["\\[\\d+\\]"]

def enumerate_directory(path):
  result = []
  for (dirpath, dirnames, filenames) in walk(path):
    for filename in filenames:
      if re.match(pattern[0], filename) is not None:
        result.append(dirpath + "\\" + filename)
    for directory in dirnames:
      result.extend(enumerate_directory(path + "\\" + directory))
    break
  return result

print(len(enumerate_directory(path)))

text_file = open("out.txt", "w", encoding='UTF-8')
text_file.write("".join(str(x)+"\n" for x in enumerate_directory(path)))
text_file.close()