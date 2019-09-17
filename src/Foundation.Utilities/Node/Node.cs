namespace Naos.Foundation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class Node<T> : IEqualityComparer, IEnumerable<T>, IEnumerable<Node<T>>
    {
        private readonly List<Node<T>> children = new List<Node<T>>();

        public Node(T value)
        {
            this.Value = value;
        }

        public Node<T> Parent { get; private set; }

        public T Value { get; set; }

        public IEnumerable<Node<T>> Ancestors
        {
            get
            {
                if (this.IsRoot)
                {
                    return Enumerable.Empty<Node<T>>();
                }

                return this.Parent.ToEnumarable().Concat(this.Parent.Ancestors);
            }
        }

        public IEnumerable<Node<T>> Descendants
        {
            get
            {
                return this.SelfAndDescendants.Skip(1);
            }
        }

        public IEnumerable<Node<T>> Children
        {
            get
            {
                return this.children;
            }
        }

        public IEnumerable<Node<T>> Siblings
        {
            get
            {
                return this.SelfAndSiblings.Where(this.Other);
            }
        }

        public IEnumerable<Node<T>> SelfAndChildren
        {
            get
            {
                return this.ToEnumarable().Concat(this.Children);
            }
        }

        public IEnumerable<Node<T>> SelfAndAncestors
        {
            get
            {
                return this.ToEnumarable().Concat(this.Ancestors);
            }
        }

        public IEnumerable<Node<T>> SelfAndDescendants
        {
            get
            {
                return this.ToEnumarable().Concat(this.Children.SelectMany(c => c.SelfAndDescendants));
            }
        }

        public IEnumerable<Node<T>> SelfAndSiblings
        {
            get
            {
                if (this.IsRoot)
                {
                    return this.ToEnumarable();
                }

                return this.Parent.Children;
            }
        }

        public IEnumerable<Node<T>> Leaves
        {
            get
            {
                return this.Descendants.Where(n => !n.Children.Any());
            }
        }

        public IEnumerable<Node<T>> All
        {
            get
            {
                return this.Root.SelfAndDescendants;
            }
        }

        public IEnumerable<Node<T>> SameLevel
        {
            get
            {
                return this.SelfAndSameLevel.Where(this.Other);
            }
        }

        public int Level
        {
            get
            {
                return this.Ancestors.Count();
            }
        }

        public IEnumerable<Node<T>> SelfAndSameLevel
        {
            get
            {
                return this.GetNodesAtLevel(this.Level);
            }
        }

        public Node<T> Root
        {
            get
            {
                return this.SelfAndAncestors.Last();
            }
        }

        public bool IsRoot
        {
            get { return this.Parent == null; }
        }

        public Node<T> this[int index]
        {
            get
            {
                return this.children[index];
            }
        }

        public static bool operator ==(Node<T> value1, Node<T> value2)
        {
            if ((object)value1 == null && (object)value2 == null)
            {
                return true;
            }

            return ReferenceEquals(value1, value2);
        }

        public static bool operator !=(Node<T> value1, Node<T> value2)
        {
            return !(value1 == value2);
        }

        public static IEnumerable<Node<T>> CreateTree<TId>(IEnumerable<T> values, Func<T, TId> idSelector, Func<T, TId/*?*/> parentIdSelector, bool ignoreOnMissingParent = false)
            //where TId : struct
        {
            if (!values.SafeAny())
            {
                return Enumerable.Empty<Node<T>>();
            }

            if (values.FirstOrDefault(v => IsSameId(idSelector(v), parentIdSelector(v))) != null)
            {
                throw new ArgumentException("at least one value has the same id and parentid");
            }

            return CreateTree(values.Select(v => new Node<T>(v)), idSelector, parentIdSelector, ignoreOnMissingParent);
        }

        public static IEnumerable<Node<T>> CreateTree<TId>(IEnumerable<Node<T>> rootNodes, Func<T, TId> idSelector, Func<T, TId/*?*/> parentIdSelector, bool ignoreOnMissingParent = false)
            //where TId : struct
        {
            var result = rootNodes.ToList();
            if (result.Duplicates(n => n).Any())
            {
                throw new ArgumentException("one or more values contains duplicate keys");
            }

            foreach (var rootNode in result)
            {
                var parentId = parentIdSelector(rootNode.Value);
                var parent = result.FirstOrDefault(n => IsSameId(idSelector(n.Value), parentId));

                if (parent != null)
                {
                    parent.Add(rootNode);
                }
                else if (parentId != null && !ignoreOnMissingParent) // there is no node with this parentId in the tree, ignore?
                {
                    throw new ArgumentException($"a node has the parent id [{parentId/*.Value*/}] but no other nodes has this id");
                }
            }

            return result.Where(n => n.IsRoot);
        }

        public Node<T> Add(T value, int index = -1)
        {
            var result = new Node<T>(value);
            this.Add(result, index);
            return result;
        }

        public void Add(Node<T> childNode, int index = -1)
        {
            if (index < -1)
            {
                throw new ArgumentException("the index can not be lower then -1");
            }

            if (index > this.Children.Count() - 1)
            {
                throw new ArgumentException($"the index ({index}) can not be higher then index of the last iten. Use the AddChild() method without an index to add at the end");
            }

            if (!childNode.IsRoot)
            {
                throw new ArgumentException("the child node cannot be added because it is not a root node");
            }

            if (this.Root == childNode)
            {
                throw new ArgumentException("the child node is the rootnode of the parent");
            }

            if (childNode.SelfAndDescendants.Any(n => this == n))
            {
                throw new ArgumentException("the child node cannot be added to itself or its descendants");
            }

            childNode.Parent = this;
            if (index == -1)
            {
                this.children.Add(childNode);
            }
            else
            {
                this.children.Insert(index, childNode);
            }
        }

        public Node<T> AddFirstChild(T value)
        {
            var result = new Node<T>(value);
            this.AddFirstChild(result);
            return result;
        }

        public void AddFirstChild(Node<T> childNode)
        {
            this.Add(childNode, 0);
        }

        public Node<T> AddFirstSibling(T value)
        {
            var result = new Node<T>(value);
            this.AddFirstSibling(result);
            return result;
        }

        public void AddFirstSibling(Node<T> childNode)
        {
            this.Parent.AddFirstChild(childNode);
        }

        public Node<T> AddLastSibling(T value)
        {
            var childNode = new Node<T>(value);
            this.AddLastSibling(childNode);
            return childNode;
        }

        public void AddLastSibling(Node<T> childNode)
        {
            this.Parent.Add(childNode);
        }

        public Node<T> AddParent(T value)
        {
            var result = new Node<T>(value);
            this.AddParent(result);
            return result;
        }

        public void AddParent(Node<T> parentNode)
        {
            if (!this.IsRoot)
            {
                throw new ArgumentException($"the node [{this.Value}] already has a parent");
            }

            parentNode.Add(this);
        }

        public IEnumerable<Node<T>> GetNodesAtLevel(int level)
        {
            return this.Root.GetNodesAtLevelInternal(level);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.children.Safe().Select(n => n.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.children.GetEnumerator();
        }

        public IEnumerator<Node<T>> GetEnumerator()
        {
            return this.children.GetEnumerator();
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public void Disconnect()
        {
            if (this.IsRoot)
            {
                throw new InvalidOperationException("a root node can not get disconnected from a parent");
            }

            this.Parent.children.Remove(this);
            this.Parent = null;
        }

        public override bool Equals(object other)
        {
            var valueThisType = other as Node<T>;
            return this == valueThisType;
        }

        public bool Equals(Node<T> value)
        {
            return this == value;
        }

        public bool Equals(Node<T> value1, Node<T> value2)
        {
            return value1 == value2;
        }

        bool IEqualityComparer.Equals(object value1, object value2)
        {
            var valueThisType1 = value1 as Node<T>;
            var valueThisType2 = value2 as Node<T>;

            return this.Equals(valueThisType1, valueThisType2);
        }

        public int GetHashCode(object obj)
        {
            return this.GetHashCode(obj as Node<T>);
        }

        public override int GetHashCode()
        {
            return this.GetHashCode(this);
        }

        public int GetHashCode(Node<T> value)
        {
            return base.GetHashCode();
        }

        //private static bool IsSameId<TId>(TId id, TId? parentId)
        //    where TId : struct
        //{
        //    return parentId != null && id.Equals(parentId.Value);
        //}

        private static bool IsSameId<TId>(TId id, TId parentId)
        {
            return parentId != null && id.Equals(parentId);
        }

        private bool Other(Node<T> node)
        {
            return !ReferenceEquals(node, this);
        }

        private IEnumerable<Node<T>> GetNodesAtLevelInternal(int level)
        {
            if (level == this.Level)
            {
                return this.ToEnumarable();
            }

            return this.Children.SelectMany(c => c.GetNodesAtLevelInternal(level));
        }
    }
}
