using System;
using System.Collections.Generic;

namespace InvisibleHand.Items.Categories
{

    public interface ICategory
    {
        int Priority { get; set; }
        int ID { get; }
        int Ordinal { get; }
        int ParentID { get; set; }

        bool Enabled { get; }

        string Name { get; }
        string QualifiedName { get; }

        void ToggleEnabled();
    }

    public interface ICategory<T> : ICategory, IComparable<ICategory<T>>, IEquatable<ICategory<T>>, IComparer<T>
    {
        ICategory<T> Parent { get; }

        bool Matches(T item);
        bool Matches(IDictionary<string, int> flags);

        ICategory<T> Category { get; }

        ICategory<T> Match(IDictionary<string, int> item_flags);
    }

    public interface IUnion<V> : ICategory
                                 where V: ICategory
    {
        ISet<V> UnionMembers { get; }

        void AddMember(V member, bool force_replace=false);
        void RemoveMember(V member);
    }

    public interface IMergeable<U, V> : ICategory
                                     where U : IUnion<V>
                                     where V : ICategory
    {
        int UnionID { get; }

        void Merge(U union);
        void Merge(int union_id);

        void Unmerge();
    }
}
