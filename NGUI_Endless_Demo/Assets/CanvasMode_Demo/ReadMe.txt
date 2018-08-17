1. 屏幕空间下：只以sort
   OverLay: UI永远显示在游戏最前方
           只以sort order来判断渲染顺序：
           sort order越小，越先渲染，即越在底层容易被后渲染的遮挡
   Camera: 用于显示3D模型加入UI界面效果的渲染模式
   
   
   
2. 世界空间下：角色的血条显示，UI作为游戏对象来使用
               先以Sorting Layer 来判断，值越小越先被渲染
               Sorting Layer相同时再以Order in Layer来判断，值越小越先被渲染