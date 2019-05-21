using UnityEngine;
using UnityEditor;
using System.Collections;

namespace shacoEditor
{
    public partial class BehaviourTreeViewer : EditorWindow
	{
		private void FormatTreesPosition()
        {
            if (null == _rootTree) return;

            _maxTreeItemRect.Set(0, 0, 0, 0);
			_rootTree.editorDrawPosition.y = _defaultGUIHeight;
            _rootTree.editorDrawPosition.position += _rootTreeMoveToCenterOffset;
            int maxDeep = 1;

            //default format
            FormatTreePosition(_rootTree, 0, 0);
            _rootTree.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
            {
                FormatTreePosition(tree, index, level);
                if (level > maxDeep) maxDeep = level;
                return true;
            });

            //deep format
            for (int i = 0; i < maxDeep; ++i)
                FormatTreesPositionWithDeepLevel();

            //Reset root position
            if (null != _rootTree.child)
            {
                var firstChildInRoot = (shaco.Base.BehaviourTree)_rootTree.child;
                var lastChildInRoot = (shaco.Base.BehaviourTree)firstChildInRoot.prev;
                var centerPosition = (firstChildInRoot.editorDrawPosition.position + lastChildInRoot.editorDrawPosition.max) / 2;
                centerPosition = new Vector2(centerPosition.x - _rootTree.editorDrawPosition.width / 2, _rootTree.editorDrawPosition.position.y);
                _rootTreeMoveToCenterOffset = _rootTree.editorDrawPosition.position - centerPosition;
                _rootTree.editorDrawPosition.position = centerPosition;
            }

            //calculate new max rect view
            _maxTreeItemRect.y += _defaultGUIHeight;
            _maxTreeItemRect.height -= _defaultGUIHeight;
        }
        
        private void FormatTreesPositionWithDeepLevel()
        {
            shaco.Base.BehaviourTree leftChild = null;
            _rootTree.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
            {
                if (tree.IsFirstChild())
                {
                    leftChild = tree;
                    CheckGetDrawMaxRect(leftChild);
                }
                else
                {
                    FixFormatTreePosition(leftChild, tree);
                    leftChild = tree;
                }
                return true;
            });
        }

        // private shaco.Base.BehaviourTree GetPrevSiblingParent(shaco.Base.BehaviourTree tree)
        // {
        //     shaco.Base.BehaviourTree retValue = tree;

        //     if (null == retValue) return retValue;
        //     retValue = retValue.parent;

        //     while (true)
        //     {
        //         if (retValue.parent.IsRoot() || retValue == null || retValue.parent == null)
        //             break;
        //         else
        //             retValue = retValue.parent;
        //     }

        //     if (null != retValue && !retValue.IsFirstChild())
        //     {
        //         retValue = retValue.prev;
        //     }

        //     return retValue;
        // }

        private void FormatTreePosition(shaco.Base.BehaviourTree tree, int index, int level)
        {
            // Debug.Log("tree=" + tree + " index=" + index + " level=" + level + " count=" + tree.parent.Count);
            var parent = (shaco.Base.BehaviourTree)tree.parent;
            Rect drawPosition;
            if (null != parent)
            {
                int count = parent.Count;
                drawPosition = new Rect(0, 0, tree.editorDrawPosition.width, tree.editorDrawPosition.height);

                if (count > 1)
                {
                    float offsetCount = count % 2 == 0 ? 0.5f : 0;
                    drawPosition.x = parent.editorDrawPosition.x + index * parent.editorDrawPosition.width - ((count / 2) - offsetCount) * parent.editorDrawPosition.width;

                    if (index < count / 2.0f)
                        drawPosition.x -= (count / 2.0f - index - 0.5f) * parent.editorDrawPosition.width;
                    else
                        drawPosition.x += (index - count / 2.0f + 0.5f) * parent.editorDrawPosition.width;
                }
                else
                    drawPosition.x = parent.editorDrawPosition.x;

                drawPosition.y = parent.editorDrawPosition.y + parent.editorDrawPosition.height + parent.editorDrawPosition.height / 5 + _minItemMargin.y;
                tree.editorDrawPosition = drawPosition;            
            }
        }

        private void FixFormatTreePosition(shaco.Base.BehaviourTree left, shaco.Base.BehaviourTree right)
        {
            var maxRightTree = left;
            var maxLeftTree = right;

            if (left != null)
            {
                left.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
                {
                    if (tree.editorDrawPosition.x > maxRightTree.editorDrawPosition.x) maxRightTree = tree;
                    return true;
                });
            }

            if (right != null)
            {
                right.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
                {
                    if (tree.editorDrawPosition.x < maxLeftTree.editorDrawPosition.x) maxLeftTree = tree;
                    return true;
                });
            }

            var offsetPosX = maxRightTree.editorDrawPosition.x - maxLeftTree.editorDrawPosition.x + maxRightTree.editorDrawPosition.width + _minItemMargin.x;
            var offsetPos = new Vector2(offsetPosX, 0);

            // Debug.Log("left=" + left + " maxRightTree=" + maxRightTree + " right=" + right + " maxLeftTree=" + maxLeftTree +  " offset=" + offsetPosX);

            //need fix position
            if (offsetPosX > 0)
            {
                if (left.IsFirstChild())
                {
                    MoveTreeDrawPosition(left, -offsetPos);
                    // var parentWithRootChild = GetParentWithRootChild(left);
                    // var parentPrev = parentWithRootChild.prev;
                    // while (!parentPrev.IsLastChild())
                    // {
                    //     MoveTreeDrawPosition(GetParentWithRootChild(parentPrev), -offsetPos);
                    //     parentPrev = parentPrev.prev;
                    // }
                }
                else
                {
                    MoveTreeDrawPosition(right, offsetPos);
                    // var parentWithRootChild = GetParentWithRootChild(right);
                    // var parentNext = parentWithRootChild.next;
                    // while (!parentNext.IsFirstChild())
                    // {
                    //     MoveTreeDrawPosition(GetParentWithRootChild(parentNext), offsetPos);
                    //     parentNext = parentNext.next;
                    // }
                }
            }

            CheckGetDrawMaxRect(left);
            CheckGetDrawMaxRect(right);
        }

        private void FixFormatTreeSizeWithGUILayout(shaco.Base.BehaviourTree tree, float minWidth = 0, float minHeight = 0)
		{
			if (Event.current.type == EventType.Repaint)
            {
                var lastRect = GUILayoutUtility.GetLastRect();
				lastRect.width = Mathf.Max(lastRect.width, minWidth);
                lastRect.height = Mathf.Max(lastRect.height, minHeight);
                float maxWidth = lastRect.x + lastRect.width;
                float maxHeight = lastRect.y + lastRect.height;
				var offset = new Vector2(maxWidth - tree.editorDrawPosition.width, maxHeight - tree.editorDrawPosition.height);

                if (offset.x != 0 || offset.y != 0)
                {
                    tree.editorDrawPosition.width += offset.x;
                    tree.editorDrawPosition.height += offset.y;
                    // tree.editorDrawPosition.x -= offfset.x;
                    // tree.editorDrawPosition.y -= offfset.y;

					if (offset.x != 0 || offset.y != 0)
					{
						_updateFormatPositionsDirty = true;
                    }
                }
            }
        }

        private void MoveTreeDrawPosition(shaco.Base.BehaviourTree tree, Vector2 moveOffset)
        {
            tree.editorDrawPosition.position += moveOffset;
            CheckGetDrawMaxRect(tree);

            //move self children
            tree.ForeachAllChildren((shaco.Base.BehaviourTree child, int index, int level) =>
            {
                child.editorDrawPosition.position += moveOffset;
                CheckGetDrawMaxRect(child);
                return true;
            });
        }

		private void CheckGetDrawMaxRect(shaco.Base.BehaviourTree tree)
        {
            var drawPosition = tree.editorDrawPosition;

            if (drawPosition.x < _maxTreeItemRect.x)
            {
                var offset = _maxTreeItemRect.x - drawPosition.x;
                _maxTreeItemRect.x -= offset;
                _maxTreeItemRect.width += offset;
            }
            if (drawPosition.y < _maxTreeItemRect.y)
            {
                var offset = _maxTreeItemRect.y - drawPosition.y;
                _maxTreeItemRect.y -= offset;
                _maxTreeItemRect.height += offset;
            }
            if (drawPosition.x + drawPosition.width - _maxTreeItemRect.x > _maxTreeItemRect.width)
            {
                _maxTreeItemRect.width = drawPosition.x + drawPosition.width - _maxTreeItemRect.x;
            }
            if (drawPosition.y + drawPosition.height - _maxTreeItemRect.y > _maxTreeItemRect.height) _maxTreeItemRect.height = drawPosition.y + drawPosition.height - _maxTreeItemRect.y;
		}
	}
}
