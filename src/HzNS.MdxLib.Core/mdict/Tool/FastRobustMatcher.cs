using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HzNS.MdxLib.MDict.Tool
{
    public class FastRobustMatcher<T>
    {
        public class Node<TT> //: Dictionary<Char, Node>
        {
            public bool Terminated { get; set; }

            //public Char C { get; set; }
            public Dictionary<char, Node<TT>> Children { get; internal set; }
            //public List<Node> Nodes { get; set; }

            public TT Data { get; set; }

            public Node()
            {
                Children = new Dictionary<char, Node<TT>>();
            }

            public Node(TT data)
            {
                Children = new Dictionary<char, Node<TT>>();
                Data = data;
            }

            //public void Add(char c, Node node){
            //    //if(Children.ContainsKey(c)){
            //    //    Node n = Children[c];
            //    //    n.Add(node);
            //    //}
            //}
        }

        Node<T> Root { get; set; }
        public bool NoCase { get; set; }

        public void Add(string s, T data)
        {
            Node<T> root = Root;
            for (int i = 0; i < s.Length; ++i)
            {
                char c = s[i];

                if (NoCase)
                    c = char.ToLower(c);

                if (!root.Children.ContainsKey(c))
                {
                    root.Children.Add(c, new FastRobustMatcher<T>.Node<T>(data));
                }

                root = root.Children[c];
            }

            if (root != Root)
            {
                root.Terminated = true;
            }
        }

        public List<Node<T>> MatchString(string s)
        {
            List<Node<T>> res = new List<Node<T>>();
            Node<T> root = Root;
            for (int i = 0; i < s.Length; ++i)
            {
                char c = s[i];
                //addChar(root, c);
                if (!root.Children.ContainsKey(c))
                {
                    return res;
                }

                res.Add(root);
                root = root.Children[c];
            }

            return null;
        }

        /// <summary>
        /// 执行大小写敏感的模糊匹配
        /// </summary>
        /// <param name="s"></param>
        /// <param name="nocase"></param>
        /// <returns></returns>
        public T Match(string s)
        {
            //List<Node<T>> res = new List<Node<T>>();
            Node<T> root = Root;
            Node<T> last = null;
            for (int i = 0; i < s.Length; ++i)
            {
                char c = s[i];
                if (NoCase) c = char.ToLower(c);
                //addChar(root, c);
                if (!root.Children.ContainsKey(c))
                    break;
                last = root.Children[c];
                root = last;
            }

            if (last == null)
            {
                throw new Exception("NOT FOUND");
            }

            return last.Data;
        }


        public FastRobustMatcher()
        {
            Root = new Node<T>();
        }

        public static void MainTest(string[] argv)
        {
            FastRobustMatcher<int> frm = new FastRobustMatcher<int>();
            frm.Add("test", 1);
            frm.Add("testc", 2);
            frm.Add("tess", 3);

            string ts = "tess";
            Debug.WriteLine("Matching '{0}':", ts);
            List<FastRobustMatcher<int>.Node<int>> res = frm.MatchString(ts);
            if (res == null)
            {
                Debug.WriteLine("    No Matched.");
            }
            else
            {
                foreach (FastRobustMatcher<int>.Node<int> n in res)
                {
                }
            }

            //Node n1, n2, n3;
            //n1 = new Node();
            ////n1.Children.Add('a', new FastRubstMatcher<Char>.Node());
            //n2 = new Node();
            //n3 = new Node();
            //frm.Root.Add('a', n1);
            ////n1.Nodes.Add()
        }
    }
}