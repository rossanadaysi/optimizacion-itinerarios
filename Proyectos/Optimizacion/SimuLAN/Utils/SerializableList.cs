using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Collections.ObjectModel;

namespace SimuLAN.Utils
{
    /// <summary>
    /// Lista serializable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [XmlRoot("list")]
    public class SerializableList<T> : ICollection<T>, IEnumerable, IXmlSerializable, IListSource
    {
        #region delegates

        public delegate void SerializableListItemHandler(T item);

        #endregion

        #region internal vars

        /// <summary>
        /// internal hashtable to stock datas.
        /// </summary>
        protected Hashtable InternalHashtable = new Hashtable();
       
        #endregion

        #region constructors

        /// <summary>
        /// Défault constructor. necessary for serialisation
        /// </summary>
        public SerializableList()
        {
        }

        /// <summary>
        /// copie ctor
        /// </summary>
        /// <param name="source"></param>
        public SerializableList(SerializableList<T> source)
        {
            this.InternalHashtable = (Hashtable)source.InternalHashtable.Clone();
        }

        /// <summary>
        /// ctor using an ICollection
        /// </summary>
        /// <param name="source"></param>
        public SerializableList(ICollection<T> source)
        {
            foreach(T item in source)
            {
                this.Add(item);
            }
        }

        /// <summary>
        /// ctor with many items .
        /// </summary>
        /// <param name="items"></param>
        /// <example>usage : SerializableList<int> A = new SerializableList<int>(1, 2, 3, 4, 5, 6);</example>
        public SerializableList(params T[] items)
        {
            foreach(T item in items)
            {
                this.Add(item);
            }
        }

        #endregion

        #region SerializableList operators

        /// <summary>
        /// Performs a "union" of the two SerializableLists, where all the elements
        /// in both SerializableLists are present.  That is, the element is included if it is in either <c>a</c> or <c>b</c>.
        /// Neither this SerializableList nor the input SerializableList are modified during the operation.  The return value
        /// is a <c>Clone()</c> of this SerializableList with the extra elements added in.
        /// </summary>
        /// <param name="a">A collection of elements.</param>
        /// <returns>A new <c>SerializableList</c> containing the union of this <c>SerializableList</c> with the specified collection.
        /// Neither of the input objects is modified by the union.</returns>
        public virtual SerializableList<T> Union(SerializableList<T> a)
        {
            SerializableList<T> resultSerializableList = (SerializableList<T>)this.Clone();
            if(a != null)
                resultSerializableList.AddAll(a);
            return resultSerializableList;
        }

        /// <summary>
        /// Performs a "union" of two SerializableLists, where all the elements
        /// in both are present.  That is, the element is included if it is in either <c>a</c> or <c>b</c>.
        /// The return value is a <c>Clone()</c> of one of the SerializableLists (<c>a</c> if it is not <c>null</c>) with elements of the other SerializableList
        /// added in.  Neither of the input SerializableLists is modified by the operation.
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <param name="b">A SerializableList of elements.</param>
        /// <returns>A SerializableList containing the union of the input SerializableLists.  <c>null</c> if both SerializableLists are <c>null</c>.</returns>
        public static SerializableList<T> Union(SerializableList<T> a, SerializableList<T> b)
        {
            if(a == null && b == null)
                return null;
            else if(a == null)
                return (SerializableList<T>)b.Clone();
            else if(b == null)
                return (SerializableList<T>)a.Clone();
            else
                return a.Union(b);
        }

        /// <summary>
        /// Performs a "union" of two SerializableLists, where all the elements
        /// in both are present.  That is, the element is included if it is in either <c>a</c> or <c>b</c>.
        /// The return value is a <c>Clone()</c> of one of the SerializableLists (<c>a</c> if it is not <c>null</c>) with elements of the other SerializableList
        /// added in.  Neither of the input SerializableLists is modified by the operation.
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <param name="b">A SerializableList of elements.</param>
        /// <returns>A SerializableList containing the union of the input SerializableLists.  <c>null</c> if both SerializableLists are <c>null</c>.</returns>
        public static SerializableList<T> operator |(SerializableList<T> a, SerializableList<T> b)
        {
            return Union(a, b);
        }

        /// <summary>
        /// Performs an "intersection" of the two SerializableLists, where only the elements
        /// that are present in both SerializableLists remain.  That is, the element is included if it exists in
        /// both SerializableLists.  The <c>Intersect()</c> operation does not modify the input SerializableLists.  It returns
        /// a <c>Clone()</c> of this SerializableList with the appropriate elements removed.
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <returns>The intersection of this SerializableList with <c>a</c>.</returns>
        public virtual SerializableList<T> Intersect(SerializableList<T> a)
        {
            SerializableList<T> resultSerializableList = (SerializableList<T>)this.Clone();
            if(a != null)
                resultSerializableList.RetainAll(a);
            else
                resultSerializableList.Clear();
            return resultSerializableList;
        }

        /// <summary>
        /// Performs an "intersection" of the two SerializableLists, where only the elements
        /// that are present in both SerializableLists remain.  That is, the element is included only if it exists in
        /// both <c>a</c> and <c>b</c>.  Neither input object is modified by the operation.
        /// The result object is a <c>Clone()</c> of one of the input objects (<c>a</c> if it is not <c>null</c>) containing the
        /// elements from the intersect operation. 
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <param name="b">A SerializableList of elements.</param>
        /// <returns>The intersection of the two input SerializableLists.  <c>null</c> if both SerializableLists are <c>null</c>.</returns>
        public static SerializableList<T> Intersect(SerializableList<T> a, SerializableList<T> b)
        {
            if(a == null && b == null)
                return null;
            else if(a == null)
            {
                return b.Intersect(a);
            }
            else
                return a.Intersect(b);
        }

        /// <summary>
        /// Performs an "intersection" of the two SerializableLists, where only the elements
        /// that are present in both SerializableLists remain.  That is, the element is included only if it exists in
        /// both <c>a</c> and <c>b</c>.  Neither input object is modified by the operation.
        /// The result object is a <c>Clone()</c> of one of the input objects (<c>a</c> if it is not <c>null</c>) containing the
        /// elements from the intersect operation. 
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <param name="b">A SerializableList of elements.</param>
        /// <returns>The intersection of the two input SerializableLists.  <c>null</c> if both SerializableLists are <c>null</c>.</returns>
        public static SerializableList<T> operator &(SerializableList<T> a, SerializableList<T> b)
        {
            return Intersect(a, b);
        }

        /// <summary>
        /// Performs a "minus" of SerializableList <c>b</c> from SerializableList <c>a</c>.  This returns a SerializableList of all
        /// the elements in SerializableList <c>a</c>, removing the elements that are also in SerializableList <c>b</c>.
        /// The original SerializableLists are not modified during this operation.  The result SerializableList is a <c>Clone()</c>
        /// of this <c>SerializableList</c> containing the elements from the operation.
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <returns>A SerializableList containing the elements from this SerializableList with the elements in <c>a</c> removed.</returns>
        public virtual SerializableList<T> Minus(SerializableList<T> a)
        {
            SerializableList<T> resultSerializableList = (SerializableList<T>)this.Clone();
            if(a != null)
                resultSerializableList.RemoveAll(a);
            return resultSerializableList;
        }

        /// <summary>
        /// Performs a "minus" of SerializableList <c>b</c> from SerializableList <c>a</c>.  This returns a SerializableList of all
        /// the elements in SerializableList <c>a</c>, removing the elements that are also in SerializableList <c>b</c>.
        /// The original SerializableLists are not modified during this operation.  The result SerializableList is a <c>Clone()</c>
        /// of SerializableList <c>a</c> containing the elements from the operation. 
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <param name="b">A SerializableList of elements.</param>
        /// <returns>A SerializableList containing <c>A - B</c> elements.  <c>null</c> if <c>a</c> is <c>null</c>.</returns>
        public static SerializableList<T> Minus(SerializableList<T> a, SerializableList<T> b)
        {
            if(a == null)
                return null;
            else
                return a.Minus(b);
        }

        /// <summary>
        /// Performs a "minus" of SerializableList <c>b</c> from SerializableList <c>a</c>.  This returns a SerializableList of all
        /// the elements in SerializableList <c>a</c>, removing the elements that are also in SerializableList <c>b</c>.
        /// The original SerializableLists are not modified during this operation.  The result SerializableList is a <c>Clone()</c>
        /// of SerializableList <c>a</c> containing the elements from the operation. 
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <param name="b">A SerializableList of elements.</param>
        /// <returns>A SerializableList containing <c>A - B</c> elements.  <c>null</c> if <c>a</c> is <c>null</c>.</returns>
        public static SerializableList<T> operator -(SerializableList<T> a, SerializableList<T> b)
        {
            return Minus(a, b);
        }

        /// <summary>
        /// Performs an "exclusive-or" of the two SerializableLists, keeping only the elements that
        /// are in one of the SerializableLists, but not in both.  The original SerializableLists are not modified
        /// during this operation.  The result SerializableList is a <c>Clone()</c> of this SerializableList containing
        /// the elements from the exclusive-or operation.
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <returns>A SerializableList containing the result of <c>a ^ b</c>.</returns>
        public virtual SerializableList<T> ExclusiveOr(SerializableList<T> a)
        {
            SerializableList<T> resultSerializableList = (SerializableList<T>)this.Clone();
            foreach(T element in a)
            {
                if(resultSerializableList.Contains(element))
                    resultSerializableList.Remove(element);
                else
                    resultSerializableList.Add(element);
            }
            return resultSerializableList;
        }

        /// <summary>
        /// Performs an "exclusive-or" of the two SerializableLists, keeping only the elements that
        /// are in one of the SerializableLists, but not in both.  The original SerializableLists are not modified
        /// during this operation.  The result SerializableList is a <c>Clone()</c> of one of the SerializableLists
        /// (<c>a</c> if it is not <c>null</c>) containing
        /// the elements from the exclusive-or operation.
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <param name="b">A SerializableList of elements.</param>
        /// <returns>A SerializableList containing the result of <c>a ^ b</c>.  <c>null</c> if both SerializableLists are <c>null</c>.</returns>
        public static SerializableList<T> ExclusiveOr(SerializableList<T> a, SerializableList<T> b)
        {
            if(a == null && b == null)
                return null;
            else if(a == null)
                return (SerializableList<T>)b.Clone();
            else if(b == null)
                return (SerializableList<T>)a.Clone();
            else
                return a.ExclusiveOr(b);
        }

        /// <summary>
        /// Performs an "exclusive-or" of the two SerializableLists, keeping only the elements that
        /// are in one of the SerializableLists, but not in both.  The original SerializableLists are not modified
        /// during this operation.  The result SerializableList is a <c>Clone()</c> of one of the SerializableLists
        /// (<c>a</c> if it is not <c>null</c>) containing
        /// the elements from the exclusive-or operation.
        /// </summary>
        /// <param name="a">A SerializableList of elements.</param>
        /// <param name="b">A SerializableList of elements.</param>
        /// <returns>A SerializableList containing the result of <c>a ^ b</c>.  <c>null</c> if both SerializableLists are <c>null</c>.</returns>
        public static SerializableList<T> operator ^(SerializableList<T> a, SerializableList<T> b)
        {
            return ExclusiveOr(a, b);
        }

        /// <summary>
        /// Cretes a SerializableList of another type and convert all elements in this type.
        /// </summary>
        /// <typeparam name="U">Convert into this Type </typeparam>
        /// <param name="converter">Converter from T to U</param>
        /// <returns>the converted SerializableList</returns>
        public SerializableList<U> ConvertAll<U>(Converter<T, U> converter)
        {
            SerializableList<U> result = new SerializableList<U>();
            foreach(T element in this)
                result.Add(converter(element));
            return result;
        }

        /// <summary>
        /// verify if the prédicate is true for each elements of the SerializableList.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public bool TrueForAll(Predicate<T> predicate)
        {
            foreach(T element in this)
                if(!predicate(element))
                    return false;
            return true;
        }

        /// <summary>
        /// Filter the SerializableList, applying the predicate to all elements. 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>Filtered SerializableList</returns>
        public SerializableList<T> FindAll(Predicate<T> predicate)
        {
            SerializableList<T> result = new SerializableList<T>();
            foreach(T element in this)
                if(predicate(element))
                    result.Add(element);
            return result;
        }

        /// <summary>
        /// apply the action to all elements
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<T> action)
        {
            foreach(T element in this)
                action(element);
        }

        #endregion

        #region standard collections operators

        /// <summary>
        /// Adds the specified element to this SerializableList if it is not already present.
        /// </summary>
        /// <param name="o">The object to add to the SerializableList.</param>
        public virtual void Add(T item)
        {
            this.Add_AndGetResult(item);
        }
 
        //void IEnumerable.Add(object o)
        //{
        //    if(o is T)
        //        this.Add_AndGetResult(o as T);
        //}

        /// <summary>
        /// Adds the specified element to this SerializableList if it is not already present.
        /// </summary>
        /// <param name="o">The object to add to the SerializableList.</param>
        /// <returns><c>true</c> is the object was added, <c>false</c> if it was already present.</returns>
        public virtual bool Add_AndGetResult(T item)
        {
            // Could be removed, if null items are necessary but be carreful using the HashCode !!
            if(item == null)
                return false;

            if(!this.InternalHashtable.Contains(item.GetHashCode()))
            {
                this.InternalHashtable.Add(item.GetHashCode(), item);
                this.OnAdded(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds all the elements in the specified collection to the SerializableList if they are not already present.
        /// </summary>
        /// <param name="c">A collection of objects to add to the SerializableList.</param>
        /// <returns><c>true</c> is the SerializableList changed as a result of this operation, <c>false</c> if not.</returns>
        public bool AddAll(SerializableList<T> c)
        {
            bool changed = false;
            foreach(T item in c)
                changed |= this.Add_AndGetResult(item);
            return changed;
        }

        /// <summary>
        /// Returns the elemnt of index index. attention, this is special because the SerializableList is not indexed.
        /// it works on a temporary picure of the SerializableList, do not loop using this if the loop can change the
        /// contents of the SerializableList (the number of element)
        /// I was needed it !
        /// </summary>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                if(index < 0 || index >= this.InternalHashtable.Count)
                    throw new IndexOutOfRangeException();
                return this.ToList()[index];
            }
        }

        /// <summary>
        /// Removes all objects from the SerializableList.
        /// </summary>
        public void Clear()
        {
            this.InternalHashtable.Clear();
        }

        /// <summary>
        /// Returns <c>true</c> if this SerializableList contains the specified element.
        /// </summary>
        /// <param name="o">The element to look for.</param>
        /// <returns><c>true</c> if this SerializableList contains the specified element, <c>false</c> otherwise.</returns>
        public bool Contains(T Value)
        {
            return InternalHashtable.ContainsValue(Value);
        }

        /// <summary>
        /// Returns <c>true</c> if the SerializableList contains all the elements in the specified collection.
        /// </summary>
        /// <param name="c">A collection of objects.</param>
        /// <returns><c>true</c> if the SerializableList contains all the elements in the specified collection, <c>false</c> otherwise.</returns>
        public bool ContainsAll(SerializableList<T> c)
        {
            foreach(T value in c)
            {
                if(!this.Contains(value))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if this SerializableList contains no elements.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.InternalHashtable.Count == 0;
            }
        }

        /// <summary>
        /// Removes the specified element from the SerializableList.
        /// </summary>
        /// <param name="o">The element to be removed.</param>
        /// <returns><c>true</c> if the SerializableList contained the specified element, <c>false</c> otherwise.</returns>
        public virtual bool Remove(T Value)
        {
            bool contained = this.Contains(Value);
            if(contained)
            {
                this.InternalHashtable.Remove(Value.GetHashCode());
                this.OnRemoved(Value);
            }
            return contained;
        }

        /// <summary>
        /// Remove all the specified elements from this SerializableList, if they exist in this SerializableList.
        /// </summary>
        /// <param name="c">A collection of elements to remove.</param>
        /// <returns><c>true</c> if the SerializableList was modified as a result of this operation.</returns>
        public bool RemoveAll(SerializableList<T> Values)
        {
            bool changed = false;
            foreach(T o in Values)
                changed |= this.Remove(o);
            return changed;
        }

        /// <summary>
        /// Retains only the elements in this SerializableList that are contained in the specified collection.
        /// </summary>
        /// <param name="c">Collection that defines the SerializableList of elements to be retained.</param>
        /// <returns><c>true</c> if this SerializableList changed as a result of this operation.</returns>
        public bool RetainAll(SerializableList<T> c)
        {
            //Put data from C into a SerializableList so we can use the Contains() method.
            SerializableList<T> cSerializableList = new SerializableList<T>(c);

            //We are going to build a SerializableList of elements to remove.
            SerializableList<T> removeSerializableList = new SerializableList<T>();

            foreach(T o in this)
            {
                //If C does not contain O, then we need to remove O from our
                //SerializableList.  We can't do this while iterating through our SerializableList, so
                //we put it into RemoveSerializableList for later.
                if(!cSerializableList.Contains(o))
                    removeSerializableList.Add(o);
            }

            return this.RemoveAll(removeSerializableList);
        }

        /// <summary>
        /// Returns a clone of the <c>SerializableList</c> instance.  This will work for derived <c>SerializableList</c>
        /// classes if the derived class implements a constructor that takes no arguments.
        /// </summary>
        /// <returns>A clone of this object.</returns>
        public virtual object Clone()
        {
            //SerializableList<T> newSerializableList = (SerializableList<T>)Activator.CreateInstance(this.GetType());
            //newSerializableList.AddAll(this);
            //return newSerializableList;
            return new SerializableList<T>(this);
        }

        /// <summary>
        /// Copies the elements in the <c>SerializableList</c> to an array.  The type of array needs
        /// to be compatible with the objects in the <c>SerializableList</c>, obviously.
        /// </summary>
        /// <param name="array">An array that will be the target of the copy operation.</param>
        /// <param name="index">The zero-based index where copying will start.</param>
        public void CopyTo(Array array, int index)
        {
            this.InternalHashtable.CopyTo(array, index);
        }

        /// <summary>
        /// The number of elements currently contained in this collection.
        /// </summary>
        public int Count
        {
            get { return this.InternalHashtable.Count; }
        }

        /// <summary>
        /// Gets an enumerator for the elements in the <c>SerializableList</c>.
        /// </summary>
        /// <returns>An <c>IEnumerator</c> over the elements in the <c>SerializableList</c>.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.InternalHashtable.Values.GetEnumerator();
        }

        /// <summary>
        /// This method will test the <c>SerializableList</c> against another <c>SerializableList</c> for "equality".
        /// In this case, "equality" means that the two SerializableLists contain the same elements.
        /// The "==" and "!=" operators are not overridden by design.  If you wish to check
        /// for "equivalent" <c>SerializableList</c> instances, use <c>Equals()</c>.  If you wish to check
        /// to see if two references are actually the same object, use "==" and "!=".  
        /// </summary>
        /// <param name="o">A <c>SerializableList</c> object to compare to.</param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            if(o == null || !(o is SerializableList<T>) || ((SerializableList<T>)o).Count != this.Count)
                return false;
            else
            {
                foreach(T elt in ((SerializableList<T>)o))
                {
                    if(!this.Contains(elt))
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// forced to override because of Equals method override.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.InternalHashtable.GetHashCode();
        }

        /// <summary>
        /// Convert into a list<T>.
        /// Be CARREFUL, working on a picture of the SerializableList, not the SerializableList.
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            List<T> res = new List<T>();
            foreach(T item in this)
            {
                res.Add(item);
            }
            return res;
        }

        /// <summary>
        /// Get an object from the SerializableList, knowing it hashcode. 
        /// Usually, you get the hashcode by object.GetHashCode(). 
        /// method getHashCode could have to be overriden (see commentaries)
        /// </summary>
        /// <param name="hashcode"></param>
        /// <returns></returns>
        public T GetObjectFromHashCode(int hashcode)
        {
            return (T)this.InternalHashtable[hashcode];
        }

        #endregion

        #region event


        /// <summary>
        /// Append when an item is added
        /// </summary>
        public event SerializableListItemHandler Added;

        protected void OnAdded(T Item)
        {
            if(this.Added != null)
                this.Added(Item);
        }


        /// <summary>
        /// Append wen an item is removed
        /// </summary>
        public event SerializableListItemHandler Removed;

        protected void OnRemoved(T Item)
        {
            if(this.Removed != null)
                this.Removed(Item);
        }



        #endregion

        #region IListSource ICollection<T> IEnumerable<T> IEnumerable Membres

        /// <summary>
        /// Get if the elements of the SerializableList implements IList
        /// </summary>
        public bool ContainsListCollection
        {
            get
            {
                if(this.Count <= 0) 
                    return false;
                return this[0] is IList; // we can suppose that all elemnt are same type. but I am not sure if T is an interface.
            }
        }

        /// <summary>
        /// Get a picture of the SerializableList, as an ilist (see commentaries)
        /// </summary>
        /// <returns></returns>
        public IList GetList()
        {
            return this.ToList();
        }


        /// <summary>
        /// Copies the elements in the <c>SerializableList</c> to an array.  The type of array needs
        /// to be compatible with the objects in the <c>SerializableList</c>, obviously.
        /// </summary>
        /// <param name="array">An array that will be the target of the copy operation.</param>
        /// <param name="index">The zero-based index where copying will start.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.CopyTo(array as Array, arrayIndex);
        }

        /// <summary>
        /// Return if the SerializableList is readonly, return false in this implementation
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// For this, i am not sure !
        /// </summary>
        /// <returns></returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.ToList().GetEnumerator();
            //return this.InternalHashtable.GetEnumerator();
        }

        /// <summary>
        /// For this, i am not sure !
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {

            //return this.ToList().GetEnumerator();
            return this.InternalHashtable.GetEnumerator();
        }

        #endregion

        #region IXmlSerializable Membres

        /// <summary>
        /// Return null.
        /// </summary>
        /// <returns></returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Read a XML element corresponding to a SerializableList of T
        /// </summary>
        /// <param name="reader"></param>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            // this line suposes that T is XmlSerializable 
            // (not an IDictionnary for example, or implements IXmlSerializable too ...)
            System.Xml.Serialization.XmlSerializer S = new System.Xml.Serialization.XmlSerializer(typeof(T));

            // Very important
            reader.Read();

            while(reader.NodeType != XmlNodeType.EndElement)
            {
                try
                {
                    // YES it use the XmlSerializer to serialize each items !
                    // It is so simple.
                    T item = (T)S.Deserialize(reader);
                    if(item != null) // May be i have to throw here ?
                        this.Add(item);
                }
                catch
                {
                    // May be i have to throw ??
                }
            }

            // Very important, if reader.Read()
            reader.ReadEndElement();
        }

        /// <summary>
        /// Write an XML Element corresponding to a SerializableList of T
        /// </summary>
        /// <param name="writer"></param>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            // this line suposes that T is XmlSerializable 
            // (not an IDictionnary for example, or implements IXmlSerializable too ...)
            System.Xml.Serialization.XmlSerializer S = new System.Xml.Serialization.XmlSerializer(typeof(T));

            foreach(T item in this.InternalHashtable.Values)
            {
                try
                {
                    // YES it use the XmlSerializer to serialize each items !
                    // It is so simple.
                    S.Serialize(writer, item, null);
                }
                catch//(Exception ex)
                {
                    // May be i have to throw ??

                    // System.Windows.Forms.MessageBox.Show(ex.ToString());
                    // writer.WriteElementString(this._TypeName, null);
                }
            }
        }
        
        #endregion
    
        #region Cast from array
        
        // I am not sure i can ad this. It work, but i am affraid about conflict in differents calls.
        // I could replace implicit by explicit, but it would not be so good.
        // and there ise a constructor using params.

        ///// <summary>
        ///// Allow to do  SerializableList<double> S = new double[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        ///// </summary>
        ///// <param name="a"></param>
        ///// <returns></returns>
        //public static implicit operator SerializableList<T>(T[] a)
        //{
        //    return new SerializableList<T>(a);
        //}

        #endregion
    }
}
