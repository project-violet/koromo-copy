###
#
#   Koromo Copy - Chromdriver downloader
#
#   Copyright (C) 2018. dc-koromo. All rights reserved.
#
###

import io
import zipfile
from contextlib import closing
import requests
#https://stackoverflow.com/questions/22340265/python-download-file-using-requests-directly-to-memory
requests.get("https://chromedriver.storage.googleapis.com/2.41/chromedriver_win32.zip" with closing(r), zipfile.ZipFile(io.BytesIO(r.content)) as archive:
  print({member.filename: archive.read(member) for member in archive.infolist()})