using DayZTypesHelper.Models;
using DayZTypesHelper.Services;

namespace DayZTypesHelper.Tests;

public class UndoRedoServiceTests
{
    [Fact]
    public void Undo_RestoresPreviousState()
    {
        var svc = new UndoRedoService();

        var original = new TypeEntry { Name = "Item", Nominal = 5 };
        svc.PushUndo(original);

        // Simulate user changing the value
        var modified = new TypeEntry { Name = "Item", Nominal = 99 };

        var restored = svc.Undo(modified);
        Assert.NotNull(restored);
        Assert.Equal(5, restored.Nominal);
    }

    [Fact]
    public void Redo_RestoresForwardState()
    {
        var svc = new UndoRedoService();

        var original = new TypeEntry { Name = "Item", Nominal = 5 };
        svc.PushUndo(original);

        var modified = new TypeEntry { Name = "Item", Nominal = 99 };
        var restored = svc.Undo(modified);
        Assert.NotNull(restored);

        // Now redo
        var redone = svc.Redo(restored);
        Assert.NotNull(redone);
        Assert.Equal(99, redone.Nominal);
    }

    [Fact]
    public void Undo_ReturnsNull_WhenEmpty()
    {
        var svc = new UndoRedoService();
        var entry = new TypeEntry { Name = "Item" };
        Assert.Null(svc.Undo(entry));
    }

    [Fact]
    public void Redo_ReturnsNull_WhenEmpty()
    {
        var svc = new UndoRedoService();
        var entry = new TypeEntry { Name = "Item" };
        Assert.Null(svc.Redo(entry));
    }

    [Fact]
    public void PushUndo_ClearsRedoStack()
    {
        var svc = new UndoRedoService();

        var v1 = new TypeEntry { Name = "Item", Nominal = 1 };
        svc.PushUndo(v1);

        var v2 = new TypeEntry { Name = "Item", Nominal = 2 };
        var undone = svc.Undo(v2);
        Assert.NotNull(undone);
        Assert.True(svc.CanRedo("Item"));

        // New change should clear redo
        svc.PushUndo(undone);
        Assert.False(svc.CanRedo("Item"));
    }

    [Fact]
    public void CanUndo_CanRedo_ReportCorrectly()
    {
        var svc = new UndoRedoService();
        Assert.False(svc.CanUndo("X"));
        Assert.False(svc.CanRedo("X"));

        svc.PushUndo(new TypeEntry { Name = "X", Nominal = 1 });
        Assert.True(svc.CanUndo("X"));
        Assert.False(svc.CanRedo("X"));
    }

    [Fact]
    public void Clear_RemovesAllHistory()
    {
        var svc = new UndoRedoService();
        svc.PushUndo(new TypeEntry { Name = "A" });
        svc.PushUndo(new TypeEntry { Name = "B" });

        svc.Clear();

        Assert.False(svc.CanUndo("A"));
        Assert.False(svc.CanUndo("B"));
    }

    [Fact]
    public void MultipleUndos_WorkInSequence()
    {
        var svc = new UndoRedoService();

        svc.PushUndo(new TypeEntry { Name = "Item", Nominal = 1 });
        svc.PushUndo(new TypeEntry { Name = "Item", Nominal = 2 });
        svc.PushUndo(new TypeEntry { Name = "Item", Nominal = 3 });

        var current = new TypeEntry { Name = "Item", Nominal = 4 };

        var r1 = svc.Undo(current);
        Assert.NotNull(r1);
        Assert.Equal(3, r1.Nominal);

        var r2 = svc.Undo(r1);
        Assert.NotNull(r2);
        Assert.Equal(2, r2.Nominal);

        var r3 = svc.Undo(r2);
        Assert.NotNull(r3);
        Assert.Equal(1, r3.Nominal);

        // No more undos
        Assert.Null(svc.Undo(r3));
    }
}
