// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;
using System.Collections.Generic;

namespace ControlFreak2
{
abstract public class ObjectPoolBase<T> 
	{
	protected List<T> 
		usedList,
		unusedList;


	// --------------------
	public ObjectPoolBase()
		{
		this.usedList = new List<T>();
		this.unusedList = new List<T>();
		}

	// ---------------------	
	abstract protected T CreateInternalObject();
	virtual protected void OnDestroyObject	(T obj)	{ }
	virtual protected void OnUseObject		(T obj) { }
	virtual protected void OnUnuseObeject		(T obj) { }


	// -----------------
	public int GetAllocatedCount()	{ return (this.usedList.Count + this.unusedList.Count); }
	public int GetUsedCount() 		{ return this.usedList.Count; }
	public int GetUnusedCount() 	{ return this.unusedList.Count; }

	public List<T> GetList()		{ return this.usedList; }
	

	// ---------------
	public T GetNewObject(int insertPos = -1)
		{ 
		if (this.unusedList.Count == 0)
			return default(T);

		T obj = this.unusedList[unusedList.Count - 1];
		this.unusedList.RemoveAt(this.unusedList.Count - 1);

		this.OnUseObject(obj);

		if (insertPos < 0)
			this.usedList.Add(obj);	
		else
			this.usedList.Insert(insertPos, obj);

		return obj;
		}


	// ---------------
	public void UnuseObject(T obj)
		{
		int i = this.usedList.IndexOf(obj);	
		if (i < 0)
			{
#if UNITY_EDITOR
			Debug.LogError("Trying to unuse unused object!! [" + ((obj != null) ? obj.ToString() : "null") + "]!");
#endif
			return;
			}

		this.usedList.RemoveAt(i);
		
		this.OnUnuseObeject(obj);

		this.unusedList.Add(obj);
		}



	// -----------------
	public void Clear()	
		{
		for (int i = 0; i < this.usedList.Count; ++i)
			this.OnUnuseObeject(this.usedList[i]);

		this.unusedList.AddRange(this.usedList);
		this.usedList.Clear(); 
		}



	// ----------------
	public void Trim(int maxCount, bool trimAtEnd = true)
		{
		if (maxCount < 0)
			maxCount = 0;

		int excessCount = this.GetUsedCount() -  maxCount;
		if (excessCount <= 0)
			return;

		int indFrom = (trimAtEnd ? (this.usedList.Count - excessCount) : 0);
		int indTo	= indFrom + excessCount;
	
		for (int i = indFrom; i < indTo; ++i)
			{ 
			T obj = this.usedList[i];
			this.OnUnuseObeject(obj);
			this.unusedList.Add(obj);
			}

		this.usedList.RemoveRange(indFrom, excessCount);		
		}



	// ----------------
	public void EnsureCapacity(int count)
		{
		if (this.GetAllocatedCount() < count)
			this.Allocate(count);
		}


	// ----------------
	public void Allocate(int count)
		{
		if (count < 0) 
			count = 0;
		
		if (count == this.GetAllocatedCount())
			return;

		// Add...

		if (count > this.GetAllocatedCount())
			{			
			if (this.usedList.Capacity < count)
				this.usedList.Capacity = count;
			if (this.unusedList.Capacity < count)
				this.unusedList.Capacity = count;

			int numToAdd = count - this.GetAllocatedCount();
			for (int i = 0; i < numToAdd; ++i)
				{
				T obj = this.CreateInternalObject();
				if (obj == null)
					throw (new System.Exception("Could not create a new object pool element [" + typeof(T).ToString() + "]!"));

				this.unusedList.Add(obj);
				}

			}

		// Remove...

		else
			{
			// First, unuse objects if needed...

			int numToUnuse = -(count - this.unusedList.Count);
			if (numToUnuse > 0)
				{
				int unuseFrom = (this.usedList.Count - numToUnuse);
				for (int i = unuseFrom; i < this.usedList.Count; ++i)
					{
					T obj = this.usedList[i];
					this.OnUnuseObeject(obj);
					this.unusedList.Add(obj);
					}
				
				this.usedList.RemoveRange(unuseFrom, numToUnuse);
				}

			// Destroy unused elements...

			int numToDestroy = this.unusedList.Count - count;
			int destroyFrom = this.unusedList.Count - numToDestroy;
	
			for (int i = destroyFrom; i < this.unusedList.Count; ++i)
				this.OnDestroyObject(this.unusedList[i]);
	
			this.unusedList.RemoveRange(destroyFrom, numToDestroy);		
			}
	
	

		}

	// ---------------------
	public void DestroyAll()
		{
		this.Allocate(0);
		}
		

	
	}
}

//! \endcond
