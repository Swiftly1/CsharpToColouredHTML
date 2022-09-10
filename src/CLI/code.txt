using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DictionaryList
{
    public class DictionaryList<T, U>
    {
        private readonly Node<T, U> Root = new Node<T, U>(default!, null) { IsRoot = true };

        /// <summary>
        /// This parameter indicates whether key contains NULLs e.g [UserA, null, new User()].
        /// Allowing NULLs within keys has some performance - speed and memory penalty, that's why it is disabled by default.
        /// </summary>
        public bool AllowNULLsInKeys { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allow_keys_with_nulls">This parameter indicates whether key contains NULLs e.g [UserA, null, new User()].
        /// Allowing NULLs within keys has some performance - speed and memory penalty, that's why it is disabled by default.</param>
        public DictionaryList(bool allow_keys_with_nulls = false)
        {
            AllowNULLsInKeys = allow_keys_with_nulls;
        }

        public void Add(List<T> data, U value)
        {
            var current = Root;

            for (int i = 0; i < data.Count; i++)
            {
                T item = data[i];

                if (!AllowNULLsInKeys && item == null)
                    throw new ArgumentException($"Element at index '{i}' is NULL. It cannot be used as a Key's element. " +
                        $"If you want to use NULLs inside Keys, then either use constructor 'DictionaryList(true)' or set property 'AllowNULLsInKeys' to true.");

                Node<T, U> found = FindNode(current, item);

                var isLast = i == data.Count - 1;

                if (found != null)
                {
                    if (isLast)
                    {
                        if (found.StoredValue is null)
                        {
                            found.StoredValue = new ValueWrapper<U>(true, value);
                        }
                        else
                        {
                            if (found.StoredValue.HasValue && !found.StoredValue.Value!.Equals(value))
                            {
                                throw new ArgumentException($"Value: '{value}' cannot be saved because there's already value:" +
                                    $" {found.StoredValue.Value}. Key: {string.Join(",", data)}");
                            }
                        }
                    }

                    current = found;
                }
                else
                {
                    var wrapper2 = new ValueWrapper<U>(isLast, value);
                    current = current.Add(item, wrapper2);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Node<T, U> FindNode(Node<T, U> current, T item)
        {
            if (AllowNULLsInKeys)
            {
                for (int i = 0; i < current.Children.Count; i++)
                {
                    if (Equals(current.Children[i].ArrayValue, item))
                        return current.Children[i];
                }
            }
            else
            {
                for (int i = 0; i < current.Children.Count; i++)
                {
                    if (current.Children[i].ArrayValue!.Equals(item))
                        return current.Children[i];
                }
            }

            return null;
        }

        public bool TryGet(List<T> data, out U? value)
        {
            var current = Root;

            for (int i = 0; i < data.Count; i++)
            {
                T item = data[i];

                Node<T, U> found = FindNode(current, item);

                var isLast = i == data.Count - 1;

                if (found != null)
                {
                    if (isLast)
                    {
                        if (found.StoredValue == null || !found.StoredValue.HasValue)
                            goto Fail;

                        value = found.StoredValue.Value;
                        return true;
                    }

                    current = found;
                }
                else
                {
                    goto Fail;
                }
            }

            Fail:
            value = default;
            return false;
        }
    }
}