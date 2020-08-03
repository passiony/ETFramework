using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

namespace ETModel
{
    public class UIPageViewLoop : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        private ScrollRect rect; //滑动组件  
        private float targethorizontal = 0; //滑动的起始坐标  
        private bool isDrag = false; //是否拖拽结束  
        private List<float> posList = new List<float>(); //求出每页的临界角，页索引从0开始  
        private int currentPageIndex = -1;
        public Action<int> OnPageChanged;
        private RectTransform content;
        private RectTransform rectTransform;
        private bool stopMove = true;
        public float smooting = 4; //滑动速度  
        public float sensitivity = 0;
        private float startTime;
        private float startDragHorizontal;


        public bool isLoop = false;

        private List<GameObject> fakeObjectList;


        void Awake()
        {
            rect = transform.GetComponent<ScrollRect>();
            content = rect.content;
            rectTransform = transform as RectTransform;
            fakeObjectList = new List<GameObject>();
        }

        private void Start()
        {
            ResetToBegin();
        }

        public void Refresh()
        {
            currentPageIndex = -1;
            targethorizontal = 0;
            posList.Clear();

            //只有一个对象不能循环
            if (content.transform.childCount <= 1)
            {
                isLoop = false;
            }

            ClearFake();
            if (isLoop)
            {
                MakeFake();
            }


            var tempWidth = ((float) content.transform.childCount * rectTransform.rect.width);
            content.sizeDelta = new Vector2(tempWidth, rectTransform.rect.height);
            //未显示的长度
            float horizontalLength = content.rect.width - rectTransform.rect.width;
            for (int i = 0; i < rect.content.transform.childCount; i++)
            {
                posList.Add(rectTransform.rect.width * i / horizontalLength);
            }
        }


        private void MakeFake()
        {
            var gameObjLast = content.transform.GetChild(content.transform.childCount - 1).gameObject;
            var gameObjFirst = content.transform.GetChild(0).gameObject;

            var first = Instantiate(gameObjLast, content);
            first.transform.SetAsFirstSibling();

            var last = Instantiate(gameObjFirst, content);
            last.transform.SetAsLastSibling();

            fakeObjectList.Add(first);
            fakeObjectList.Add(last);
        }

        private void ClearFake()
        {
            for (int i = 0; i < fakeObjectList.Count; i++)
            {
                DestroyImmediate(fakeObjectList[i]);
            }

            fakeObjectList.Clear();
        }


        void Update()
        {

            if (!isDrag && !stopMove)
            {
                startTime += Time.deltaTime;
                float t = startTime * smooting;
                rect.horizontalNormalizedPosition = Mathf.Lerp(rect.horizontalNormalizedPosition, targethorizontal, t);
                if (t >= 1)
                {
                    stopMove = true;
                    if (isLoop)
                    {
                        if (currentPageIndex == 0)
                        {
                            pageTo(posList.Count - 2);
                        }
                        else if (currentPageIndex == posList.Count - 1)
                        {
                            pageTo(1);
                        }
                    }
                }
            }
        }

        public void ResetToBegin()
        {
            Refresh();
            pageTo(isLoop ? 1 : 0);
        }

        public void ResetTo(int page)
        {
            Refresh();
            pageTo(page);
        }

        public void pageTo(int index, bool animate = false)
        {
            if (isLoop)
            {
                if (index < 0)
                {
                    index = posList.Count - 1;
                }
                else if (index > posList.Count - 1)
                {
                    index = 0;
                }
            }
            else
            {
                index = Mathf.Clamp(index, 0, posList.Count-1);    
            }

            if (animate)
            {
                isDrag = false;
                startTime = 0;
                stopMove = false;
                targethorizontal = posList[index];
            }
            else
            {
                rect.horizontalNormalizedPosition = posList[index];
            }
            
            SetPageIndex(index);
            
        }

        public void pageToNext()
        {
            pageTo(currentPageIndex + 1, true);
            OnPageChanged?.Invoke(0);
        }

        public void pageToFront()
        {
            pageTo(currentPageIndex - 1, true);
            OnPageChanged?.Invoke(0);
        }

        private void SetPageIndex(int index)
        {
            if (currentPageIndex != index)
            {
                currentPageIndex = index;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!stopMove)
            {
                return;
            }

            isDrag = true;
            //开始拖动
            startDragHorizontal = rect.horizontalNormalizedPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!stopMove)
            {
                return;
            }

            float delta = eventData.delta.x;
            int index = currentPageIndex;
            if (Mathf.Abs(delta) > 30)
                index -= (int) Mathf.Sign(delta);
            else
                index = NearestPageIndex();

            if (isLoop)
            {
                if (index < 0)
                {
                    index = posList.Count - 1;
                }
                else if (index > posList.Count - 1)
                {
                    index = 0;
                }
            }
            else
            {
                index = Mathf.Clamp(index, 0, posList.Count-1);    
            }
            SetPageIndex(index);
            targethorizontal = posList[index]; //设置当前坐标，更新函数进行插值  
            isDrag = false;
            startTime = 0;
            stopMove = false;
            OnPageChanged?.Invoke(0);
        }

        int NearestPageIndex()
        {
            float posX = rect.horizontalNormalizedPosition;
            posX += ((posX - startDragHorizontal) * sensitivity);
            posX = Mathf.Clamp(posX, 0, 1);

            int index = 0;
            float offset = Mathf.Abs(posList[index] - posX);

            for (int i = 1; i < posList.Count; i++)
            {
                float temp = Mathf.Abs(posList[i] - posX);
                if (temp < offset)
                {
                    index = i;
                    offset = temp;
                }
            }

            return index;
        }
    }
}