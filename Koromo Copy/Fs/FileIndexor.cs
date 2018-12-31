/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Koromo_Copy.Fs
{
    /// <summary>
    /// 모든 파일과 폴더를 트리형태 그대로 가져오는 도구입니다.
    /// </summary>
    public class FileIndexor
    {
        FileIndexorNode node;
        List<Tuple<string, UInt64, FileInfo[]>> directory_list = new List<Tuple<string, UInt64, FileInfo[]>>();
        List<Tuple<string, string>> error_list = new List<Tuple<string, string>>();
        string root_directory;

        bool listing_files = false;

        public string RootDirectory { get { return root_directory; } set { root_directory = value; } }
        public FileIndexorNode Node { get { return node?.Nodes?[0]; } }
        public int Count { get { return directory_list.Count; } }
        public bool OnlyListing { get; set; }
        public List<Tuple<string, UInt64, FileInfo[]>> Directories { get { return directory_list; } }

        /// <summary>
        /// 파일인덱서를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            directory_list.Clear();
            error_list.Clear();
            node = null;
        }

        /// <summary>
        /// 모든 폴더를 나열합니다.
        /// </summary>
        /// <param name="target_directory"></param>
        /// <returns></returns>
        public async Task ListingDirectoryAsync(string target_directory)
        {
            root_directory = target_directory;
            await Task.Run(() => prelistingFolder(target_directory));
            directory_list.Sort();
            node = new FileIndexorNode(target_directory, 0, null);
            await Task.Run(() => createNodes());
        }
        
        #region [--- Listing ---]

        /// <summary>
        /// 파일시스템 트리를 생성하기전 모든 폴더 목록을 디스크에서 불러옵니다.
        /// </summary>
        /// <param name="path"></param>
        private void prelistingFolder(string path)
        {
            try
            {
                UInt64 folder_size = 0;

                FileInfo[] file_info = null;
                if (!OnlyListing)
                {
                    file_info = new DirectoryInfo(path).GetFiles();
                    foreach (FileInfo f in file_info)
                        folder_size += (UInt64)f.Length;
                }

                if (!path.EndsWith("\\")) path = path + "\\";

                lock (directory_list)
                {
                    directory_list.Add(new Tuple<string, UInt64, FileInfo[]>(path, folder_size, file_info));
                }

                Parallel.ForEach(Directory.GetDirectories(path), n => listingFolder(n));
            }
            catch (Exception ex)
            {
                error_list.Add(new Tuple<string, string>(path, ex.ToString()));
                Monitor.Instance.Push($"{path} {ex.Message}");
            }
        }

        private void listingFolder(string path)
        {
            try
            {
                UInt64 folder_size = 0;

                FileInfo[] file_info = null;
                if (!OnlyListing)
                {
                    file_info = new DirectoryInfo(path).GetFiles();
                    foreach (FileInfo f in file_info)
                        folder_size += (UInt64)f.Length;
                }

                lock (directory_list)
                {
                    directory_list.Add(new Tuple<string, UInt64, FileInfo[]>(path + "\\", folder_size, file_info));
                }

                Parallel.ForEach(Directory.GetDirectories(path), n => listingFolder(n));
            }
            catch (Exception ex)
            {
                lock (error_list)
                error_list.Add(new Tuple<string, string>(path, ex.ToString()));
                Monitor.Instance.Push($"{path} {ex.Message}");
            }
        }

        private Int32 index = 0;

        /// <summary>
        /// 만들어진 폴더 목록을 기반으로 트리 노드를 작성합니다.
        /// </summary>
        private void createNodes()
        {
            for (; index < directory_list.Count - 1; index++)
            {
                FileIndexorNode _node = new FileIndexorNode(directory_list[index].Item1, directory_list[index].Item2, directory_list[index].Item3);
                if (directory_list[index + 1].Item1.Contains(directory_list[index].Item1))
                {
                    node.AddItem(_node);
                    index += 1;
                    createNodesRecursize(ref _node);
                    break;
                }
            }
        }

        private void createNodesRecursize(ref FileIndexorNode parent_node)
        {
            for (; index < directory_list.Count; index++)
            {
                if (directory_list[index].Item1.Contains(parent_node.Path))
                {
                    FileIndexorNode m = new FileIndexorNode(directory_list[index].Item1, directory_list[index].Item2, directory_list[index].Item3);
                    parent_node.AddItem(m);
                    if (index < directory_list.Count - 1 &&
                        directory_list[index + 1].Item1.Contains(directory_list[index].Item1))
                    {
                        index++;
                        createNodesRecursize(ref m);
                    }
                    parent_node.Size += m.Size;
                }
                else
                {
                    index--;
                    break;
                }
            }
        }

        #endregion

        /// <summary>
        /// 경로를 이용해 노드를 가져옵니다.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FileIndexorNode GetPathNode(string path)
        {
            string[] seperated = path.Split('\\');
            string section = "";
            FileIndexorNode n = node;
            for (int i = 0; i < seperated.Length; i++)
            {
                section += seperated[i] + '\\';
                foreach (FileIndexorNode nd in n.Nodes)
                    if (nd.Path == section)
                    { n = nd; break; }
            }
            return n;
        }

        /// <summary>
        /// 최상위 노드를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public FileIndexorNode GetRootNode()
        {
            return GetPathNode(root_directory);
        }

        /// <summary>
        /// 모든 폴더목록을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public List<string> GetDirectories()
        {
            List<string> result = new List<string>();
            directory_list.ForEach(n => result.Add(n.Item1));
            return result;
        }

        /// <summary>
        /// 하위폴더를 제외한 순수 폴더 사이즈를 기준으로 오름차순으로 정렬한 리스트를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string, UInt64, FileInfo[]>> GetListSortWithNativeSize()
        {
            List<Tuple<string, UInt64, FileInfo[]>> r = new List<Tuple<string, UInt64, FileInfo[]>>(directory_list);
            r.Sort((n1, n2) => {
                try
                {
                    return n2.Item2.CompareTo(n1.Item2);
                }
                catch { return 1; }
            });
            return r;
        }

        /// <summary>
        /// 루트 폴더의 크기를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public UInt64 GetTotalSize()
        {
            return node.Nodes[0].Size;
        }

        #region [--- Recursion ---]

        public delegate void EnumeratorDelegate(string path, List<FileInfo> files);

        /// <summary>
        /// 모든 노드를 재귀적으로 탐색합니다.
        /// </summary>
        /// <param name="func"></param>
        public void Enumerate(EnumeratorDelegate func, bool subdir = true)
        {
            foreach (FileIndexorNode node in GetRootNode().Nodes)
            {
                func(node.Path, node.Files);
                if (subdir == true)
                    enumerate(func, node);
            }
        }

        private void enumerate(EnumeratorDelegate func, FileIndexorNode parent)
        {
            foreach (FileIndexorNode node in parent.Nodes)
            {
                enumerate(func, node);
                func(node.Path, node.Files);
            }
        }
        
        #endregion
    }
}
