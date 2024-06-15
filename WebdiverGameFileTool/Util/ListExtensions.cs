﻿using System.Numerics;

namespace WebdiverGameFileTool.Util;

public static class ListExtensions {
    public static int AddAndGetIndex<T>(this IList<T> list, T value) {
        lock (list) {
            list.Add(value);
            return list.Count - 1;
        }
    }

    public static int AddRangeAndGetIndex<T>(this List<T> list, IEnumerable<T> value) {
        lock (list) {
            var i = list.Count;
            list.AddRange(value);
            return i;
        }
    }

    public static Vector3 ToVector3(this IEnumerable<float> value) {
        using var a = value.GetEnumerator();
        return new() {
            X = a.MoveNext() ? a.Current : throw new ArgumentOutOfRangeException(nameof(value), value, null),
            Y = a.MoveNext() ? a.Current : throw new ArgumentOutOfRangeException(nameof(value), value, null),
            Z = a.MoveNext() ? a.Current : throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    public static Quaternion ToQuaternion(this IEnumerable<float> value) {
        using var a = value.GetEnumerator();
        return new() {
            X = a.MoveNext() ? a.Current : throw new ArgumentOutOfRangeException(nameof(value), value, null),
            Y = a.MoveNext() ? a.Current : throw new ArgumentOutOfRangeException(nameof(value), value, null),
            Z = a.MoveNext() ? a.Current : throw new ArgumentOutOfRangeException(nameof(value), value, null),
            W = a.MoveNext() ? a.Current : throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }
}