using DayZTypesHelper.Models;

namespace DayZTypesHelper.Services;

/// <summary>
/// Per-classname undo/redo stacks. Stores deep-cloned snapshots of TypeEntry.
/// </summary>
public sealed class UndoRedoService
{
    private const int MaxStackDepth = 50;

    private readonly Dictionary<string, Stack<TypeEntry>> _undoStacks = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Stack<TypeEntry>> _redoStacks = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Push a snapshot before a change is applied.</summary>
    public void PushUndo(TypeEntry entry)
    {
        var key = entry.Name;

        if (!_undoStacks.TryGetValue(key, out var stack))
        {
            stack = new Stack<TypeEntry>();
            _undoStacks[key] = stack;
        }

        if (stack.Count >= MaxStackDepth)
        {
            // Trim oldest entries by rebuilding the stack
            var items = stack.ToArray();
            stack.Clear();
            for (var i = Math.Min(items.Length - 1, MaxStackDepth - 2); i >= 0; i--)
            {
                stack.Push(items[i]);
            }
        }

        stack.Push(entry.Clone());

        // Clear redo on new change
        if (_redoStacks.ContainsKey(key))
        {
            _redoStacks[key].Clear();
        }
    }

    /// <summary>Undo: restore previous snapshot, push current to redo.</summary>
    public TypeEntry? Undo(TypeEntry current)
    {
        var key = current.Name;

        if (!_undoStacks.TryGetValue(key, out var undoStack) || undoStack.Count == 0)
            return null;

        // Push current state to redo
        if (!_redoStacks.TryGetValue(key, out var redoStack))
        {
            redoStack = new Stack<TypeEntry>();
            _redoStacks[key] = redoStack;
        }

        redoStack.Push(current.Clone());

        return undoStack.Pop();
    }

    /// <summary>Redo: restore next snapshot, push current to undo.</summary>
    public TypeEntry? Redo(TypeEntry current)
    {
        var key = current.Name;

        if (!_redoStacks.TryGetValue(key, out var redoStack) || redoStack.Count == 0)
            return null;

        // Push current state to undo (without clearing redo)
        if (!_undoStacks.TryGetValue(key, out var undoStack))
        {
            undoStack = new Stack<TypeEntry>();
            _undoStacks[key] = undoStack;
        }

        undoStack.Push(current.Clone());

        return redoStack.Pop();
    }

    public bool CanUndo(string classname) =>
        _undoStacks.TryGetValue(classname, out var s) && s.Count > 0;

    public bool CanRedo(string classname) =>
        _redoStacks.TryGetValue(classname, out var s) && s.Count > 0;

    public void Clear()
    {
        _undoStacks.Clear();
        _redoStacks.Clear();
    }
}
