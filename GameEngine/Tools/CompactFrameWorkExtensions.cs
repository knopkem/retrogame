using System;
using System.Collections;
using System.Collections.Generic;

namespace GameEngine.Tools
{
    public class CompactFrameWorkExtensions
    {
        //Type parameter T in angle brackets.

        #region Nested type: GenericList

        public class GenericList<T> : IEnumerable<T>
        {
            protected Node Current;
            protected Node Head;

            // Nested class is also generic on T

            public GenericList() //constructor
            {
                Head = null;
            }

            // Implementation of the iterator

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator()
            {
                Node current = Head;
                while (current != null)
                {
                    yield return current.Data;
                    current = current.Next;
                }
            }

            // IEnumerable<T> inherits from IEnumerable, therefore this class 
            // must implement both the generic and non-generic versions of 
            // GetEnumerator. In most cases, the non-generic method can 
            // simply call the generic method.
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            public void AddHead(T t) //T as method parameter type
            {
                var n = new Node(t) {Next = Head};
                Head = n;
            }

            #region Nested type: Node

            protected class Node
            {
                public Node next;

                public Node(T t) //T used in non-generic constructor
                {
                    next = null;
                    Data = t;
                }

                public Node Next
                {
                    get { return next; }
                    set { next = value; }
                }

                public T Data //T as return type of property
                { get; set; }
            }

            #endregion
        }

        #endregion

        #region Nested type: SortedList

        public class SortedList<T> : GenericList<T> where T : IComparable<T>
        {
            // A simple, unoptimized sort algorithm that 
            // orders list elements from lowest to highest:

            public void BubbleSort()
            {
                if (null == Head || null == Head.Next)
                {
                    return;
                }
                bool swapped;

                do
                {
                    Node previous = null;
                    Node current = Head;
                    swapped = false;

                    while (current.next != null)
                    {
                        //  Because we need to call this method, the SortedList
                        //  class is constrained on IEnumerable<T>
                        if (current.Data.CompareTo(current.next.Data) > 0)
                        {
                            Node tmp = current.next;
                            current.next = current.next.next;
                            tmp.next = current;

                            if (previous == null)
                            {
                                Head = tmp;
                            }
                            else
                            {
                                previous.next = tmp;
                            }
                            previous = tmp;
                            swapped = true;
                        }
                        else
                        {
                            previous = current;
                            current = current.next;
                        }
                    }
                } while (swapped);
            }
        }

        #endregion
    }
}