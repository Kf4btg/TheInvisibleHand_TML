using System;
using System.Collections.Generic;

namespace InvisibleHand.Items.Categories.Types
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



    public interface IUnion<T> : ICategory
    {
        ISet<IMergeable<T>> UnionMembers { get; }

        void AddMember(IMergeable<T> member, bool force_replace=false);
        void RemoveMember(IMergeable<T> member);
    }

    //  where U : IUnion<V>
    // public interface IMergeable<T> : ICategory
    // {
    //
    //     IUnion<T> CurrentUnion { get; }
    //     int UnionID { get; }
    //
    //     void Merge(IUnion<T> union);
    //     void Merge(int union_id);
    //
    //     void Unmerge();
    // }

    public interface ICategory<T> : ICategory, IComparable<ICategory<T>>, IEquatable<ICategory<T>>, IComparer<T>
    {
        ICategory<T> Parent { get; }

        bool Matches(T item);
        bool Matches(IDictionary<string, int> flags);

        ICategory<T> Category { get; }

        ICategory<T> Match(IDictionary<string, int> item_flags);
    }

    public interface IMergeable<T> : ICategory<T>
    {
        IUnion<T> CurrentUnion { get; }
        int UnionID { get; }

        void Merge(IUnion<T> union);
        void Merge(int union_id);

        void Unmerge();
    }
}
