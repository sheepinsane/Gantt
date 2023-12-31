﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Braincase.GanttChart
{
    /// <summary>
    /// Wrapper ProjectManager class
    /// </summary>
    [Serializable]
    public class ProjectManager : ProjectManager<Task, object>
    {
    }

    /// <summary>
    /// Concrete ProjectManager class for the IProjectManager interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    [Serializable]
    public class ProjectManager<T, R> : IProjectManager<T, R>
        where T : Task
        where R : class
    {
        HashSet<T> _mRegister = new HashSet<T>();
        List<T> _mRootTasks = new List<T>();
        Dictionary<T, List<T>> _mTaskGroups = new Dictionary<T, List<T>>();
        Dictionary<T, HashSet<T>> _mDependents = new Dictionary<T, HashSet<T>>();
        Dictionary<T, HashSet<R>> _mResources = new Dictionary<T, HashSet<R>>();
        Dictionary<T, List<T>> _mSplitTasks = new Dictionary<T, List<T>>();
        Dictionary<T, T> _mSplitTaskOfPart = new Dictionary<T, T>();
        Dictionary<T, T> _mParentOfChild = new Dictionary<T, T>();
        Dictionary<T, int> _mTaskIndices = new Dictionary<T, int>();

        /// <summary>
        /// Create a new Project
        /// </summary>
        public ProjectManager()
        {
            Now = 0;
            Start = DateTime.Now;
            TimeScale = GanttChart.TimeScale.Day;
        }

        /// <summary>
        /// Get or set the period we are at now
        /// </summary>
        public int Now { get; set; }

        /// <summary>
        /// Get or set the starting date for this project
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Get or set the time scale on this project. Each period on the task represents one unit of TimeScale.
        /// </summary>
        public TimeScale TimeScale { get; set; }

        /// <summary>
        /// Get the date after the specified period based on TimeScale
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int period)
        {
            DateTime datetime = DateTime.Now;
            if (this.TimeScale == TimeScale.Day)
            {
                datetime = this.Start.AddDays(period);
            }
            else if (this.TimeScale == TimeScale.Week)
            {
                datetime = this.Start.AddDays(period * 7 - (int)this.Start.DayOfWeek);
            }
            return datetime;
        }

        /// <summary>
        /// Create a new T for this Project and add it to the T tree
        /// </summary>
        /// <returns></returns>
        public void Add(T task)
        {
            if (!this._mRegister.Contains(task))
            {
                _mRegister.Add(task);
                _mRootTasks.Add(task);
                _mTaskGroups[task] = new List<T>();
                _mDependents[task] = new HashSet<T>();
                _mResources[task] = new HashSet<R>();
                _mParentOfChild[task] = null;
            }
        }

        /// <summary>
        /// Remove task from this Project
        /// </summary>
        /// <param name="task"></param>
        public void Delete(T task)
        {
            if (task != null
                && !_mSplitTaskOfPart.ContainsKey(task) // not a task part
                )
            {
                // Check if is group so can ungroup the task
                if (this.IsGroup(task))
                    this.Ungroup(task);

                if (this.IsSplit(task))
                    this.Merge(task);

                // Really delete all references
                _mRootTasks.Remove(task);
                _mTaskGroups.Remove(task);
                _mDependents.Remove(task);
                _mResources.Remove(task);
                _mParentOfChild.Remove(task);
                _mSplitTasks.Remove(task);
                foreach (var g in _mTaskGroups) g.Value.Remove(task); // optimised: no need to check for contains
                foreach (var g in _mDependents) g.Value.Remove(task);
                _mRegister.Remove(task);
            }
            else if (task != null
                && _mSplitTaskOfPart.ContainsKey(task) // must be existing part
                )
            {
                var split = _mSplitTaskOfPart[task];
                var parts = _mSplitTasks[split];
                if (parts.Count > 2)
                {
                    parts.Remove(task); // remove the part from the split task
                    _mRegister.Remove(task); // unregister the part
                    _mResources.Remove(task);
                    _mSplitTaskOfPart.Remove(task); // remove the reverse lookup
                    
                    split.Start = parts.First().Start; // recalculate the split task
                    split.End = parts.Last().End;
                    split.Duration = split.End - split.Start;
                }
                else
                {
                    this.Merge(split);
                }
            }
        }

        /// <summary>
        /// Add the member T to the group T
        /// </summary>
        /// <param name="group"></param>
        /// <param name="member"></param>
        public void Group(T group, T member)
        {
            if (group != null
                && member != null
                && _mRegister.Contains(group)
                )
            {
                // change the member to become the split task is member is a task part
                if (_mSplitTaskOfPart.ContainsKey(member)) member = _mSplitTaskOfPart[member];

                if (_mRegister.Contains(member)
                    && !group.Equals(member)
                    && !_mSplitTasks.ContainsKey(group) // group cannot be split task
                    && !_mSplitTaskOfPart.ContainsKey(group) // group cannot be parts
                    && !this.DecendantsOf(member).Contains(group)
                    && !this.HasRelations(group)
                    )
                {
                    _LeaveParent(member);
                    _mTaskGroups[group].Add(member);
                    _mParentOfChild[member] = group;

                    _RecalculateAncestorsSchedule();
                    _RecalculateSlack();

                    // clear indices since positions changed
                    _mTaskIndices.Clear();
                }
            }
        }

        /// <summary>
        /// Remove the member task from its group
        /// </summary>
        public void Ungroup(T group, T member)
        {
            if (group != null
                && member != null
                && _mRegister.Contains(group)
                )
            {
                // change the member to become the split task is member is a task part
                if (_mSplitTaskOfPart.ContainsKey(member)) member = _mSplitTaskOfPart[member];
                if (_mRegister.Contains(member) && this.IsGroup(group))
                {
                    var ancestor = this.AncestorsOf(group).LastOrDefault();
                    if(ancestor == null) // group is in root
                        _mRootTasks.Insert(_mRootTasks.IndexOf(group) + 1, member);
                    else // group is not in root, we get the ancestor that is in root
                        _mRootTasks.Insert(_mRootTasks.IndexOf(ancestor) + 1, member);
                    _mTaskGroups[group].Remove(member);
                    _mParentOfChild[member] = null;

                    _RecalculateAncestorsSchedule();
                }
            }
        }

        /// <summary>
        /// Ungroup all member task under the specfied group task. The specified group task will become a normal task.
        /// </summary>
        /// <param name="group"></param>
        public void Ungroup(T group)
        {
            List<T> list;
            if (group != null
                //&& _mRegister.Contains(group)
                && _mTaskGroups.TryGetValue(group, out list))
            {
                var newgroup = this.ParentOf(group);
                if (newgroup == null)
                {
                    foreach (var member in list)
                    {
                        _mRootTasks.Add(member);
                        _mParentOfChild[member] = null;
                    }
                }
                else
                {
                    foreach (var member in list)
                    {
                        _mTaskGroups[newgroup].Add(member);
                        _mParentOfChild[member] = null;
                    }
                }

                list.Clear();

                _RecalculateAncestorsSchedule();
            }
        }
        
        /// <summary>
        /// Get the zero-based index of the task in this Project
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public int IndexOf(T task)
        {
            if (_mRegister.Contains(task))
            {
                if (_mTaskIndices.ContainsKey(task))
                    return _mTaskIndices[task];

                int i = 0;
                foreach (var x in Tasks)
                {
                    if (x.Equals(task))
                    {
                        _mTaskIndices[task] = i;
                        return i;
                    }
                    i++;
                }
            }
            return -1;
        }

        /// <summary>
        /// Re-position the task by offset amount of places
        /// </summary>
        /// <param name="task"></param>
        /// <param name="offset"></param>
        public void Move(T task, int offset)
        {
            if (task != null && _mRegister.Contains(task) && offset != 0)
            {
                int indexoftask = IndexOf(task);
                if (indexoftask > -1)
                {
                    int newindexoftask = indexoftask + offset;
                    // check for out of index bounds
                    if (newindexoftask < 0) newindexoftask = 0;
                    else if (newindexoftask > Tasks.Count()) newindexoftask = Tasks.Count();
                    // get the index of the task that will be displaced
                    var displacedtask = Tasks.ElementAtOrDefault(newindexoftask);

                    if (displacedtask == null)
                    {
                        // adding to the end of the task list
                        _LeaveParent(task);
                        _mRootTasks.Add(task);

                        // clear indices since positions changed
                        _mTaskIndices.Clear();
                    }
                    else if (!displacedtask.Equals(task))
                    {
                        int indexofdestinationtask;
                        var displacedtaskparent = this.ParentOf(displacedtask);
                        if (displacedtaskparent == null) // displacedtask is in root
                        {
                            indexofdestinationtask = _mRootTasks.IndexOf(displacedtask);
                            _LeaveParent(task);
                            _mRootTasks.Insert(indexofdestinationtask, task);
                        }
                        else if (!displacedtaskparent.Equals(task)) // displaced task is not under the moving task
                        {
                            var memberlist = _mTaskGroups[displacedtaskparent];
                            indexofdestinationtask = memberlist.IndexOf(displacedtask);
                            _LeaveParent(task);
                            memberlist.Insert(indexofdestinationtask, task);
                            _mParentOfChild[task] = displacedtaskparent;
                        }

                        // clear indices since positions changed
                        _mTaskIndices.Clear();
                    }
                    else // displacedtask == task, no need to move    
                    {

                    }
                }
            }
        }

        /// <summary>
        /// Get the T tree
        /// </summary>
        public IEnumerable<T> Tasks
        {
            get
            {
                var stack = new Stack<T>(1024);
                var rstack = new Stack<T>(30);
                foreach (var task in _mRootTasks)
                {
                    stack.Push(task);
                    while (stack.Count > 0)
                    {
                        var visited = stack.Pop();
                        yield return visited;

                        foreach (var member in _mTaskGroups[visited])
                            rstack.Push(member);

                        while (rstack.Count > 0) stack.Push(rstack.Pop());
                    }
                }
            }
        }
        
        /// <summary>
        /// Enumerate through all the children and grandchildren of the specified group
        /// </summary>
        public IEnumerable<T> AncestorsOf(T task)
        {
            T parent = ParentOf(task);
            while (parent != null)
            {
                yield return parent;
                parent = ParentOf(parent);
            }
        }

        /// <summary>
        /// Enumerate through all the children and grandchildren of the specified group
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public IEnumerable<T> DecendantsOf(T task)
        {
            if (_mRegister.Contains(task))
            {
                Stack<T> stack = new Stack<T>(20);
                Stack<T> rstack = new Stack<T>(10);
                foreach (var child in _mTaskGroups[task])
                {
                    stack.Push(child);
                    while (stack.Count > 0)
                    {
                        var visitedchild = stack.Pop();
                        yield return visitedchild;

                        // push the grandchild
                        rstack.Clear();
                        foreach (var grandchild in _mTaskGroups[visitedchild])
                            rstack.Push(grandchild);

                        // put in the right visiting order
                        while (rstack.Count > 0)
                            stack.Push(rstack.Pop());
                    }
                }
            }
        }

        /// <summary>
        /// Enumerate through all the direct children of the specified group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<T> ChildrenOf(T group)
        {
            if (group == null) yield break;

            List<T> list;
            if (_mTaskGroups.TryGetValue(group, out list))
            {
                var iter = list.GetEnumerator();
                while (iter.MoveNext()) yield return iter.Current;
            }
        }

        /// <summary>
        /// Enumerate through all the direct precedents and indirect precedents of the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public IEnumerable<T> PrecedentsOf(T task)
        {
            if (_mRegister.Contains(task))
            {
                var stack = new Stack<T>(20);
                foreach (var p in DirectPrecedentsOf(task))
                {
                    stack.Push(p);
                    while (stack.Count > 0)
                    {
                        var visited = stack.Pop();
                        yield return visited;
                        foreach (var grandp in DirectPrecedentsOf(visited))
                            stack.Push(grandp);
                    }
                }
            }
        }

        /// <summary>
        /// Enumerate through all the direct dependants and indirect dependants of the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public IEnumerable<T> DependantsOf(T task)
        {
            if (!_mDependents.ContainsKey(task)) yield break;

            var stack = new Stack<T>(20);
            foreach (var d in _mDependents[task])
            {
                stack.Push(d);
                while (stack.Count > 0)
                {
                    var visited = stack.Pop();
                    yield return visited;
                    foreach (var grandd in _mDependents[visited])
                        stack.Push(grandd);
                }
            }
        }

        /// <summary>
        /// Enumerate through all the direct precedents of the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public IEnumerable<T> DirectPrecedentsOf(T task)
        {
            return _mDependents.Where(x => x.Value.Contains(task)).Select(x => x.Key);
        }

        /// <summary>
        /// Enumerate through all the direct dependants of the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public IEnumerable<T> DirectDependantsOf(T task)
        {
            if (task == null) yield break;

            HashSet<T> list;
            if (_mDependents.TryGetValue(task, out list))
            {
                var iter = list.GetEnumerator();
                while (iter.MoveNext()) yield return iter.Current;
            }
        }

        /// <summary>
        /// Enumerate through all tasks that is a precedent, having dependants.
        /// </summary>
        public IEnumerable<T> Precedents
        {
            get { return _mDependents.Where(x => _mDependents[x.Key].Count > 0).Select(x => x.Key); }
        }

        /// <summary>
        /// Enumerate list of critical paths in Project
        /// </summary>
        public IEnumerable<IEnumerable<T>> CriticalPaths
        {
            get
            {
                Dictionary<int, List<T>> endtimelookp = new Dictionary<int, List<T>>(1024);
                List<T> list;
                var max_end = int.MinValue;
                foreach (var task in this.Tasks)
                {
                    if (!endtimelookp.TryGetValue(task.End, out list))
                        endtimelookp[task.End] = new List<T>(10);
                    endtimelookp[task.End].Add(task);

                    if(task.End > max_end) max_end = task.End;
                }

                if (max_end != int.MinValue)
                {
                    foreach (var task in endtimelookp[max_end])
                    {
                        yield return new T[] { task }.Concat(PrecedentsOf(task));
                    }
                }
            }
        }

        /// <summary>
        /// Get the parent group of the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public T ParentOf(T task)
        {
            if (_mParentOfChild.ContainsKey(task)) // _mRegister.Contains(task))
            {
                return _mParentOfChild[task];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get whether the specified task is a group
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool IsGroup(T task)
        {
            List<T> list;
            if (_mTaskGroups.TryGetValue(task, out list))
                return list.Count > 0;
            else
                return false;
        }

        /// <summary>
        /// Get whether the specified task is a member
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool IsMember(T task)
        {
            return this.ParentOf(task) != null;
        }

        /// <summary>
        /// Get whether the specified task has relations, either has dependants or has precedents connecting to it.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool HasRelations(T task)
        {
            if (_mRegister.Contains(task) && _mDependents.ContainsKey(task))
            {
                return _mDependents[task].Count > 0 || DirectPrecedentsOf(task).FirstOrDefault() != null;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set a relation between the precedent and dependant task
        /// </summary>
        /// <param name="precedent"></param>
        /// <param name="dependant"></param>
        public void Relate(T precedent, T dependant)
        {
            if (_mRegister.Contains(precedent)
                && _mRegister.Contains(dependant)
                )
            {
                if (_mSplitTaskOfPart.ContainsKey(precedent)) precedent = _mSplitTaskOfPart[precedent];
                if (_mSplitTaskOfPart.ContainsKey(dependant)) dependant = _mSplitTaskOfPart[dependant];

                if (!precedent.Equals(dependant)
                    &&!this.DependantsOf(dependant).Contains(precedent)
                    && !this.IsGroup(precedent)
                    && !this.IsGroup(dependant)
                    )
                {
                    _mDependents[precedent].Add(dependant);

                    _RecalculateDependantsOf(precedent);
                    _RecalculateAncestorsSchedule();
                    _RecalculateSlack();
                }
            }
        }
        
        /// <summary>
        /// Unset the relation between the precedent and dependant task, if any.
        /// </summary>
        /// <param name="precedent"></param>
        /// <param name="dependant"></param>
        public void Unrelate(T precedent, T dependant)
        {
            if (_mRegister.Contains(precedent) && _mRegister.Contains(dependant))
            {
                if (_mSplitTaskOfPart.ContainsKey(precedent)) precedent = _mSplitTaskOfPart[precedent];
                if (_mSplitTaskOfPart.ContainsKey(dependant)) dependant = _mSplitTaskOfPart[dependant];

                _mDependents[precedent].Remove(dependant);

                _RecalculateSlack();
            }
        }

        /// <summary>
        /// Remove all dependant task from specified precedent task
        /// </summary>
        /// <param name="precedent"></param>
        public void Unrelate(T precedent)
        {
            if (_mRegister.Contains(precedent))
            {
                if (_mSplitTaskOfPart.ContainsKey(precedent))
                    precedent = _mSplitTaskOfPart[precedent];

                _mDependents[precedent].Clear();

                _RecalculateSlack();
            }
        }

        /// <summary>
        /// Assign the specified resource to the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="resource"></param>
        public void Assign(T task, R resource)
        {
            if (_mRegister.Contains(task) && !_mResources[task].Contains(resource))
                _mResources[task].Add(resource);
        }

        /// <summary>
        /// Unassign the specified resource from the specfied task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="resource"></param>
        public void Unassign(T task, R resource)
        {
            _mResources[task].Remove(resource);
        }

        /// <summary>
        /// Unassign the all resources from the specfied task
        /// </summary>
        /// <param name="task"></param>
        public void Unassign(T task)
        {
            if(_mRegister.Contains(task))
                _mResources[task].Clear();
        }

        /// <summary>
        /// Unassign the specified resource from all tasks that has this resource assigned
        /// </summary>
        /// <param name="resource"></param>
        public void Unassign(R resource)
        {
            foreach (var r in _mResources.Where(x => x.Value.Contains(resource)))
                r.Value.Remove(resource);
        }

        /// <summary>
        /// Enumerate through all the resources that has been assigned to some task.
        /// </summary>
        public IEnumerable<R> Resources
        {
            get
            {
                return _mResources.SelectMany(x => x.Value).Distinct();
            }
        }

        /// <summary>
        /// Enumerate through all the resources that has been assigned to the specified task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public IEnumerable<R> ResourcesOf(T task)
        {
            if (task == null || !_mRegister.Contains(task))
                yield break;

            HashSet<R> list;
            if (_mResources.TryGetValue(task, out list))
            {
                foreach (var item in list)
                    yield return item;
            }
        }

        /// <summary>
        /// Enumerate through all the tasks that has the specified resource assigned to it.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public IEnumerable<T> TasksOf(R resource)
        {
            return _mResources.Where(x => x.Value.Contains(resource)).Select(x => x.Key);
        }

        /// <summary>
        /// Set the start value. Affects group start/end and dependants start time.
        /// </summary>
        public void SetStart(T task, int value)
        {
            if (_mRegister.Contains(task) && value != task.Start && !this.IsGroup(task))
            {
                _SetStartHelper(task, value);

                _RecalculateAncestorsSchedule();
                _RecalculateSlack();
            }
        }

        /// <summary>
        /// Set the end time. Affects group end and dependants start time.
        /// </summary>
        public void SetEnd(T task, int value)
        {
            if (_mRegister.Contains(task) && value != task.End && !this.IsGroup(task))
            {
                this._SetEndHelper(task, value);

                _RecalculateAncestorsSchedule();
                _RecalculateSlack();
            }
        }

        /// <summary>
        /// Set the duration of the specified task from start to end.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="duration">Number of timescale units between ProjectManager.Start</param>
        public void SetDuration(T task, int duration)
        {
            this.SetEnd(task, task.Start + duration);
        }

        /// <summary>
        /// Set the percentage complete of the specified task from 0.0f to 1.0f.
        /// No effect on group tasks as they will get the aggregated percentage complete of all child tasks
        /// </summary>
        /// <param name="task"></param>
        /// <param name="complete"></param>
        public void SetComplete(T task, float complete)
        {
            if (_mRegister.Contains(task)
                && complete != task.Complete
                && !this.IsGroup(task) // not a group
                && !_mSplitTasks.ContainsKey(task) // not a split task
                )
            {
                _SetCompleteHelper(task, complete);

                _RecalculateComplete();
            }
        }

        /// <summary>
        /// Set whether to collapse the specified group task. No effect on regular tasks.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="collasped"></param>
        public void SetCollapse(T task, bool collasped)
        {
            if (_mRegister.Contains(task) && this.IsGroup(task))
            {
                task.IsCollapsed = collasped;
            }
        }
        
        /// <summary>
        /// Split the specified task into consecutive parts part1 and part2.
        /// </summary>
        /// <param name="task">The regular task to split which has duration of at least 2 to make two parts of 1 time unit duration each.</param>
        /// <param name="part1">New Task part (1) of the split task, with the start time of the original task and the specified duration value.</param>
        /// <param name="part2">New Task part (2) of the split task, starting 1 time unit after part (1) ends and having the remaining of the duration of the origina task.</param>
        /// <param name="duration">The duration of part (1) will be set to the specified duration value but will also be adjusted to approperiate value if necessary.</param>
        public void Split(T task, T part1, T part2, int duration)
        {
            if (task != null
                && part1 != null
                && part2 != null
                && !part1.Equals(part2) // parts cannot be the same
                && _mRegister.Contains(task) // task must be registered
                && !_mSplitTasks.ContainsKey(task) // task must not already be a split task
                && !_mSplitTaskOfPart.ContainsKey(task) // task must not be a task part
                && _mTaskGroups[task].Count == 0 // task cannot be a group
                && !_mRegister.Contains(part1) // part1 and part2 must have never existed
                && !_mRegister.Contains(part2)
                )
            {
                _mRegister.Add(part1);  // register part1
                _mResources[part1] = new HashSet<R>(); // create container for holding resource
                
                // add part1 to split task
                task.Complete = 0.0f; // reset the complete status
                var parts = _mSplitTasks[task] = new List<T>(2); 
                parts.Add(part1);
                _mSplitTaskOfPart[part1] = task; // make a reverse lookup

                // allign the schedule
                if (duration >= task.Duration) duration--;
                part1.Start = task.Start;
                part1.End = task.End;
                part1.Duration = task.Duration;

                // split part1 to give part2
                this.Split(part1, part2, duration);
            }
        }
        
        /// <summary>
        /// Split the specified part and obtain another part from it.
        /// </summary>
        /// <param name="part">The task part to split which has duration of at least 2 to make two parts of 1 time unit duration each. Its duration will be set to the specified duration value.</param>
        /// <param name="other">New Task part of the original part, starting 1 time unit after it ends and having the remaining of the duration of the original part.</param>
        /// <param name="duration">The duration of part (1) will be set to the specified duration value but will also be adjusted to approperiate value if necessary.</param>
        public void Split(T part, T other, int duration)
        {
            if (part != null
                && other != null
                && _mSplitTaskOfPart.ContainsKey(part) // part must be an existing part
                && !_mRegister.Contains(other) // other must not have existed
                )
            {
                _mRegister.Add(other); // register other part
                _mResources[other] = new HashSet<R>(); // create container for holding resource

                var split = _mSplitTaskOfPart[part]; // get the split task
                var parts = _mSplitTasks[split]; // get the list of ordered parts

                parts.Insert(parts.IndexOf(part) + 1, other); // insert the other part after the existing part
                _mSplitTaskOfPart[other] = split; // set the reverse lookup

                if (part.Duration < 2) part.Duration = 2; // increase duration to allow for split

                if (duration < 1) duration = 1; // limit the duration point within the split task duration
                else if (duration >= part.Duration) duration = part.Duration - 1;

                // the real split
                var one_duration = duration;
                var two_duration = part.Duration - duration;
                part.Duration = one_duration;
                part.End = part.Start + one_duration;
                other.Duration = two_duration;
                other.Start = part.End + 1;
                other.End = other.Start + two_duration;

                _PackPartsForward(parts);
                split.Start = parts.First().Start; // recalculate the split task
                split.End = parts.Last().End;
                split.Duration = split.End - split.Start;

                _RecalculateDependantsOf(split);
                _RecalculateAncestorsSchedule();                
            }
        }
        
        /// <summary>
        /// Join part1 and part2 in a split task into a single part represented by part1, and part2 will be deleted from the ProjectManager.
        /// The resulting part will have a duration total of the two parts.
        /// Part1 and part2 must be actual parts and must be consecutive parts in the split task.
        /// If the join results in only one part remaining, the all parts will be deleted and the split task will promote to a regular task
        /// Schedule of other parts will not be affected.
        /// TODO: Join option: EarlyStartLateEnd, EarlyStartEarlyEnd, LateStartLateEnd
        /// </summary>
        /// <param name="part1">The part to keep in the ProjectManager after the join completes successfully.</param>
        /// <param name="part2">The part to join into part1 and be deleted afterwards from the ProjectManager.</param>
        public void Join(T part1, T part2)
        {
            if (part1 != null
                && part2 != null
                && _mSplitTaskOfPart.ContainsKey(part1) // part1 and part2 must already be existing parts
                && _mSplitTaskOfPart.ContainsKey(part2) 
                && _mSplitTaskOfPart[part1] == _mSplitTaskOfPart[part2] // part1 and part2 must be of the same split task
                )
            {

                var split = _mSplitTaskOfPart[part1];
                var parts = _mSplitTasks[split];
                if (parts.Count > 2)
                {
                    // Aggregate part2 into part1, and determine join type
                    int min; bool join_backwards;
                    if (part1.Start < part2.Start) { min = part1.Start; join_backwards = true; }
                    else { min = part2.Start; join_backwards = false; }
                    int duration = part1.Duration + part2.Duration;

                    part1.Start = min;
                    part1.Duration = duration;
                    part1.End = min + duration;

                    // aggregate resouces
                    // TODO: Ask whether to aggregate resources?
                    foreach (var r in this.ResourcesOf(part2))
                        this.Assign(part1, r);
                    this.Unassign(part2);

                    // remove all traces of part2
                    parts.Remove(part2);
                    _mResources.Remove(part2);
                    _mSplitTaskOfPart.Remove(part2);
                    _mRegister.Remove(part2);

                    // pack the remaining parts
                    if (join_backwards) _PackPartsForward(parts);
                    else _PackPartsBackwards(parts);

                    // set the duration
                    split.End = parts.Last().End;
                    split.Duration = split.End - split.Start;
                    split.Start = parts.First().Start;

                    _RecalculateAncestorsSchedule();
                }
                else
                {
                    this.Merge(split);
                }
            }
        }
        
        /// <summary>
        /// Merge all the parts of the splitted task back into one task, having duration equal to sum of total duration of individual task parts, and aggregating the resources onto the resulting task.
        /// </summary>
        /// <param name="split">The split Task to merge</param>
        public void Merge(T split)
        {
            if (split != null
                && _mSplitTasks.ContainsKey(split) // must be existing split task
                )
            {
                int duration = 0;
                _mSplitTasks[split].ForEach(x => {

                    // sum durations
                    duration += x.Duration;

                    // merge resources onto split task
                    foreach (var r in _mResources[x])
                        this.Assign(split, r);

                    // remove traces of all parts
                    _mSplitTaskOfPart.Remove(x);
                    _mRegister.Remove(x);
                    _mResources.Remove(x);
                });
                _mSplitTasks.Remove(split); // remove split as a split task

                // set the duration
                this.SetDuration(split, duration);
            }
        }
        
        /// <summary>
        /// Get the parts of the split task
        /// </summary>
        /// <param name="split"></param>
        /// <returns></returns>
        public IEnumerable<T> PartsOf(T split)
        {
            if (split != null
                && _mSplitTasks.ContainsKey(split) // must be existing split task
                )
            {
                return _mSplitTasks[split].Select(x => x);
            }
            else
            {
                return new T[0];
            }
        }
        
        /// <summary>
        /// Get the split task that the specified part belogs to.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public T SplitTaskOf(T part)
        {
            if (_mSplitTaskOfPart.ContainsKey(part))
                return _mSplitTaskOfPart[part];
            return null;
        }
        
        /// <summary>
        /// Get whether the specified task is a split task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool IsSplit(T task)
        {
            return task != null && _mSplitTasks.ContainsKey(task);
        }
        
        /// <summary>
        /// Get whether the specified task is a part of a split task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool IsPart(T task)
        {
            return task != null && _mSplitTaskOfPart.ContainsKey(task);
        }

        /// <summary>
        /// Leave the parent group if task is a member, but remain registered in ProjectManager
        /// </summary>
        /// <param name="task"></param>
        private void _LeaveParent(T task)
        {
            var parent = this.ParentOf(task);
            if (parent == null)
                _mRootTasks.Remove(task);
            else
            {
                _mTaskGroups[parent].Remove(task);
                _mParentOfChild[task] = null;
            }
        }

        private void _SetStartHelper(T task, int value)
        {
            if (task.Start != value)
            {
                if (_mSplitTaskOfPart.ContainsKey(task)) 
                {
                    // task part belonging to a split task needs special treatment
                    _SetPartStartHelper(task, value);
                }
                else // regular task or a split task, which we will treat normally
                {
                    // check out of bounds
                    if (value < 0) value = 0;
                    if (this.DirectPrecedentsOf(task).Any())
                    {
                        var max_end = this.DirectPrecedentsOf(task).Max(x => x.End);
                        if (value <= max_end) value = max_end + 1;
                    }

                    // save offset just in case we need to use for moving task parts
                    var offset = value - task.Start;

                    // cache value
                    task.Duration = task.End - task.Start;
                    task.Start = value;

                    // affect self
                    task.End = task.Start + task.Duration;

                    // calculate dependants
                    _RecalculateDependantsOf(task);

                    // shift the task parts accordingly if task was a split task
                    if (_mSplitTasks.ContainsKey(task))
                    {
                        _mSplitTasks[task].ForEach(x => {
                            x.Start += offset;
                            x.End += offset;
                        });
                    }
                }
            }
        }

        private void _SetEndHelper(T task, int value)
        {
            if (task.End != value)
            {
                if (_mSplitTaskOfPart.ContainsKey(task))
                {
                    // task part belonging to a split task needs special treatment
                    _SetPartEndHelper(task, value);
                }
                else // regular task or a split task, which we will treat normally
                {
                    // check bounds
                    bool isSplitTask = _mSplitTasks.ContainsKey(task);
                    T last_part = null;
                    if (isSplitTask)
                    {
                        last_part = _mSplitTasks[task].Last();
                        if (value <= last_part.Start) value = last_part.Start + 1;
                    }
                    if (value <= task.Start) value = task.Start + 1; // end cannot be less than start
                    
                    // assign end value
                    task.End = value;
                    task.Duration = task.End - task.Start;

                    _RecalculateDependantsOf(task);

                    if (isSplitTask)
                    {
                        last_part.End = value;
                        last_part.Duration = last_part.End - last_part.Start;
                    }
                }
            }
        }

        private void _SetPartStartHelper(T part, int value)
        {
            var split = _mSplitTaskOfPart[part];
            var parts = _mSplitTasks[split];

            // check bounds
            if (this.DirectPrecedentsOf(split).Any())
            {
                var max_end = this.DirectPrecedentsOf(split).Max(x => x.End);
                if (value < max_end) value = max_end + 1;
            }
            if (value < 0) value = 0;

            // flag whether we need to pack parts forward or backwards
            bool backwards = value < part.Start;

            // assign start value, maintining duration and modifying end
            var duration = part.End - part.Start;
            part.Start = value;
            part.End = value + duration;

            // pack packs
            if (backwards) _PackPartsBackwards(parts);
            else _PackPartsForward(parts);

            // recalculate the split
            split.Start = parts.First().Start; // recalculate the split task
            split.End = parts.Last().End;
            split.Duration = split.End - split.Start;
        }

        private void _SetPartEndHelper(T part, int value)
        {
            var split = _mSplitTaskOfPart[part];
            var parts = _mSplitTasks[split];

            // check for bounds
            if (value <= part.Start) value = part.Start + 1;

            // flag whether duration is increased or reduced
            bool increased = value > part.End;

            // set end value and duration
            part.End = value;
            part.Duration = part.End - part.Start;

            // pack parts
            if (increased) _PackPartsForward(parts);

            // recalculate the split
            split.Start = parts.First().Start; // recalculate the split task
            split.End = parts.Last().End;
            split.Duration = split.End - split.Start;

            _RecalculateDependantsOf(split);
        }

        private void _PackPartsBackwards(List<T> parts)
        {
            // pack backwards first before packing forward again
            for (int i = parts.Count - 2; i > 0; i--) // Cannot pack beyond first part (i > 0)
            {
                var earlier = parts[i];
                var later = parts[i + 1];
                if (later.Start <= earlier.End)
                {
                    earlier.End = later.Start - 1;
                    earlier.Start = earlier.End - earlier.Duration;
                }
            }

            _PackPartsForward(parts);
        }

        private void _PackPartsForward(List<T> parts)
        {
            for (int i = 1; i < parts.Count; i++)
            {
                var current = parts[i];
                var previous = parts[i - 1];
                if (previous.End >= current.Start)
                {
                    current.Start = previous.End + 1;
                    current.End = current.Start + current.Duration;
                }
            }
        }

        private void _SetCompleteHelper(T task, float value)
        {
            if (task.Complete != value)
            {
                if (value > 1) value = 1;
                else if (value < 0) value = 0;
                task.Complete = value;

                if (_mSplitTaskOfPart.ContainsKey(task))
                {
                    var split = _mSplitTaskOfPart[task];
                    var parts = _mSplitTasks[split];
                    float complete = 0;
                    int duration = 0;
                    foreach (var part in parts)
                    {
                        complete += part.Complete * part.Duration;
                        duration += part.Duration;
                    }
                    split.Complete = complete / duration;
                }
            }
        }

        private void _RecalculateComplete()
        {
            Stack<T> groups = new Stack<T>();
            foreach (var task in _mRootTasks.Where(x => this.IsGroup(x)))
            {
                _RecalculateCompletedHelper(task);
            }
        }

        private float _RecalculateCompletedHelper(T groupOrSplit)
        {
            float t_complete = 0;
            int t_duration = 0;

            if (_mSplitTasks.ContainsKey(groupOrSplit))
            {
                foreach (var part in _mSplitTasks[groupOrSplit])
                {
                    t_complete += part.Complete * part.Duration;
                    t_duration += part.Duration;
                }
            }
            else
            {
                foreach (var member in this.ChildrenOf(groupOrSplit))
                {
                    t_duration += member.Duration;
                    if (this.IsGroup(member)) t_complete += _RecalculateCompletedHelper(member) * member.Duration;
                    else t_complete += member.Complete * member.Duration;
                }
            }

            groupOrSplit.Complete = t_complete / t_duration;
            

            return groupOrSplit.Complete;
        }

        private void _RecalculateDependantsOf(T precedent)
        {
            // affect decendants
            foreach (var dependant in this.DirectDependantsOf(precedent))
            {
                if (dependant.Start <= precedent.End)
                    this._SetStartHelper(dependant, precedent.End + 1);
            }
        }

        private void _RecalculateAncestorsSchedule()
        {
            // affects parent group
            foreach(var group in _mRootTasks.Where(x => this.IsGroup(x)))
            {
                _RecalculateAncestorsScheduleHelper(group);
            }
        }

        private void _RecalculateAncestorsScheduleHelper(T group)
        {
            float t_complete = 0;
            int t_duration = 0;
            var start = int.MaxValue;
            var end = int.MinValue;
            foreach (var member in this.ChildrenOf(group))
            {
                if (this.IsGroup(member))
                    _RecalculateAncestorsScheduleHelper(member);

                t_duration += member.Duration;
                t_complete += member.Complete * member.Duration;
                if (member.Start < start) start = member.Start;
                if (member.End > end) end = member.End;
            }

            this._SetStartHelper(group, start);
            this._SetEndHelper(group, end);
            this._SetCompleteHelper(group, t_complete / t_duration);
        }

        private void _RecalculateSlack()
        {
            var max_end = this.Tasks.Max(x => x.End);
            foreach (var task in this.Tasks)
            {
                // affects slack for current task
                if (this.DirectDependantsOf(task).Any())
                {
                    // slack until the earliest dependant needs to start
                    var min = this.DirectDependantsOf(task).Min(x => x.Start);
                    task.Slack = min - task.End - 1;
                }
                else
                {
                    // no dependants, so we have all the time until the last task ends
                    task.Slack = max_end - task.End;
                }
            }
        }

        
    }
}
