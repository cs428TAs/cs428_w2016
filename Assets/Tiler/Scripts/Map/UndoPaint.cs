using System.Collections.Generic;
using UnityEngine;

namespace Tiler
{
    public class UndoPaint
    {
        public const int MaxUndos = 10;

        private class UndoData
        {
            public readonly Cell Cell;
            public readonly Point LocalTileID;
            public readonly Brush Brush;

            public UndoData(Cell cell, Point localTileID, Brush brush)
            {
                Cell = cell;
                LocalTileID = localTileID;
                Brush = brush;
            }
        }

        private readonly TilerMapEdit _me;
        private List<List<UndoData>> _undo = new List<List<UndoData>>();
        private int _index = -1;

        public UndoPaint(TilerMapEdit me)
        {
            _me = me;
        }

        public void Clear()
        {
            _undo = new List<List<UndoData>>();
            _index = -1;
        }

        public void NewUndo()
        {
            // remove any redos
            for (var i = _index + 1; i < _undo.Count; i++)
            {
                _undo.RemoveAt(i);
                i--;
            }

            // If last is empty, just use that
            if (_index != -1 && _undo[_index].Count == 0)
                return;

            _undo.Add(new List<UndoData>());

            if (_undo.Count > 10)
            {
                _undo.RemoveAt(0);
            }
            else
            {
                _index++;
            }
        }
        public void RemoveLast()
        {
            _undo.RemoveAt(_index);
            _index--;
        }

        private void NewRedo()
        {
            _undo.Insert(_index+1, new List<UndoData>());
        }
        public void PushUndo(Cell cell, Point localTileID, Brush brush)
        {
            var ud = new UndoData(cell, localTileID, brush);

            _undo[_index].Add(ud);
        }

        public void PerformUndo()
        {
            if (_index < 0) return;

            List<UndoData> undos = _undo[_index];

            RemoveLast();
            NewRedo();

            var applyList = new HashSet<Texture2D>();

            // Temp increase index for apply
            _index++;

            foreach (var undo in undos)
            {
                var changedTexture = _me.ChangeTile(undo.Cell, undo.LocalTileID, undo.Brush);
                applyList.Add(changedTexture);
            }

            // Lower it back down for undo
            _index--;

            // Apply any changes
            foreach (var t in applyList)
            {
                t.Apply();
            }
        }

        public void PerformRedo()
        {
            if (_index == _undo.Count-1) return;

            _index++;

            List<UndoData> redos = _undo[_index];

            RemoveLast();
            NewRedo();
            _index++;

            var applyList = new HashSet<Texture2D>();

            foreach (var undo in redos)
            {
                var changedTexture = _me.ChangeTile(undo.Cell, undo.LocalTileID, undo.Brush);
                applyList.Add(changedTexture);
            }

            // Apply any changes
            foreach (var t in applyList)
            {
                t.Apply();
            }
        }

        public bool IsRedo()
        {
            return _index != _undo.Count - 1;
        }
        public bool IsUndo()
        {
            return _index != -1;
        }
    }
}
