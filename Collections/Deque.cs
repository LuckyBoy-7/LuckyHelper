using System;
using System.Collections;
using System.Collections.Generic;

namespace Lucky.Kits.Collections
{
    public class Deque<T> : IEnumerable<T>
    {
        private T[] lst;

        // 我们规定front和back重合的时候满了
        private int front = 0;
        private int back = 1;
        private int capacity = 10;

        public int Count { get; private set; }
        public bool IsReadOnly { get; }

        public Deque()
        {
            lst = new T[capacity];
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return lst[(front + capacity + 1 + i) % capacity];
        }

        public T this[int i] => lst[(front + capacity + 1 + i) % capacity];

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Append(T item)
        {
            CheckMakeMakeSureCapacity();
            lst[back] = item;
            back = (back + 1) % capacity;
            Count += 1;
        }

        public void AppendLeft(T item)
        {
            CheckMakeMakeSureCapacity();
            lst[front] = item;
            front = (front - 1 + capacity) % capacity;
            Count += 1;
        }

        public T Pop()
        {
            if (Count == 0)
                throw new Exception("There's no more value!");

            back = (back - 1 + capacity) % capacity;
            T retval = lst[back];
            Count -= 1;
            return retval;
        }

        public T PopLeft()
        {
            if (Count == 0)
                throw new Exception("There's no more value!");

            front = (front + 1) % capacity;
            T retval = lst[front];
            Count -= 1;
            return retval;
        }

        private void CheckMakeMakeSureCapacity()
        {
            if ((front - 1 + capacity) % capacity == back)
            {
                capacity *= 2;
                T[] newLst = new T[capacity];
                int i = 1;
                foreach (var item in this)
                {
                    newLst[i++] = item;
                }

                lst = newLst;
                front = 0;
                back = i;
            }
        }

        public void Clear()
        {
            front = 0;
            back = 1;
            Count = 0;
        }

        public bool Contains(T item)
        {
            foreach (var v in this)
            {
                if (v.Equals(item))
                    return true;
            }

            return false;
        }
    }
}