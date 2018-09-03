/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Koromo_Copy.Fs
{
    /// <summary>
    /// 파일인덱서 트리의 노드 구조입니다.
    /// </summary>
    public class FileIndexorNode
    {
        string now_path;
        UInt64 size;
        UInt64 this_size;
        List<FileIndexorNode> nodes = new List<FileIndexorNode>();
        List<FileInfo> files;

        public string Path { get { return now_path; } }
        public UInt64 Size { get { return size; } set { size = value; } }
        public UInt64 ThisSize { get { return this_size; } }
        public List<FileIndexorNode> Nodes { get { return nodes; } }
        public List<FileInfo> Files { get { return files; } }

        public FileIndexorNode(string path, UInt64 size, bool listing_files = false)
        {
            now_path = path;
            this.this_size = this.size = size;
            if (listing_files)
            {
                files = new List<FileInfo>();
                Task.Run(() =>
                {
                    foreach (FileInfo f in new DirectoryInfo(path).GetFiles())
                        files.Add(f);
                });
            }
        }

        /// <summary>
        /// 하위 노드를 추가합니다.
        /// </summary>
        /// <param name="node"></param>
        public void AddItem(FileIndexorNode node)
        {
            nodes.Add(node);
        }

        /// <summary>
        /// 해당 경로가 존재하는지 확인합니다.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Exist(string path)
        {
            foreach (FileIndexorNode n in nodes)
                if (n.Path == path)
                    return true;
            return false;
        }

        /// <summary>
        /// 하위 노드를 포함한 총 파일 크기를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public UInt64 GetTotalSize()
        {
            UInt64 v = this_size;
            foreach (FileIndexorNode fin in nodes)
                v += fin.GetTotalSize();
            return v;
        }

        /// <summary>
        /// 사이즈로 정렬한 목록을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public List<FileIndexorNode> GetListSortWithSize()
        {
            List<FileIndexorNode> r = new List<FileIndexorNode>(nodes);
            r.Sort((n1, n2) => n2.Size.CompareTo(n1.Size));
            return r;
        }

        /// <summary>
        /// 가장 최근에 접근한 파일의 시간을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public DateTime LastFileAccessTime()
        {
            return new DirectoryInfo(now_path).GetFiles()
                .OrderByDescending(f => f.LastWriteTime).First().LastAccessTime;
        }

        /// <summary>
        /// 가장 최근에 접근한 파일이나 폴더의 시간을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public DateTime LastAccessTime()
        {
            return new DirectoryInfo(now_path).GetFileSystemInfos()
                .OrderByDescending(f => f.LastWriteTime).First().LastAccessTime;
        }
    }
}
