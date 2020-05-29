# PSD导入为UGUI工具使用文档


----
## 介绍
- 将PSD文件转换为Unity中的UGUI

## 美术使用规范
- psd中图层需要进行栅格化
- 不使用智能对象

### PSD命名规范
- 单个组内不可重名
- psd文件名与界面名要相同
- 分组名与图层名使用英文小写单词
- 文件名、图层名与组名不可以使用字符"-"
- 组名与图层名尽量使用全小写的单个单词，使用多个单词使用"_"进行连接

### 特殊类型
#### 九宫格图
##### 简介
- 效果图图层会决定图片在界面中的大小，不会导出图片资源，九宫格图图层会导出资源
##### 使用规范
- 单独建组
- 组名加后缀"@9"
- 效果图图层名字加后缀"@preview"
- 九宫格图图层名字加后缀"@png9"


#### 文字图层
##### 使用规范
- 文字图层也需要进行栅格化
- 图层命名加后缀"@t"

#### 按钮
##### 使用规范
- 单独建组
- 组名加后缀"@b"
- 按钮常态图片必须存在，图层放在组内，命名后缀"@normal"
- 按钮按下态图片可以不存在，图层放在组内，命名后缀"@pressed"
- 按钮不可点击态图片可以不存在，图层放在组内，命名后缀"@disabled"

#### 列表
- 单独建组
- 组名加后缀"@l"
- 列表需要有一张背景图图层，这一图层会直接决定列表的布局大小，命名后缀"@bg"
- 列表中的单项栅格化为单一图层，其详细布局结构需要创建新的Psd文件
- 列表可以容纳多行时至少放满一行
- 列表中的单项命名后缀"@{行号}_{列号}"  
例：  
第一行第一列的名字为"xxx@1_1"  
第一行第二列的名字为"xxx@1_2"  
第二行第一列为名字为"xxx@2_1"  
第二行第二列的名字为"xxx@2_2"

#### 勾选框
- 单独建组
- 组名加后缀"@s"
- 组内必须有一张未选中态的图层，命名后缀"@normal"
- 组内必须有一张选中态的图层，命名后缀"@select"

## 程序使用规范
### 使用方式
- 在Canvas下创建一个空的GameObject
- 为其添加组件PSD2UI
- Asset项拖入PSD文件
- Export Folder项拖入Unity内导出文件夹
- 点击“显示预览效果”可以生成预览图
- 点击“导出图片”可以将图片资源导出到导出文件夹中
- 点击“生成UI界面”可以生成界面，并将图片对应到导出文件夹中的图片

### PSD2UI组件
#### Asset
- 用于生成UI的PSD文件
#### Export Folder
- 存储导出图片的文件夹
#### Alpha
- 预览图的透明度
#### Draw Gizmos
- 绘制UI各个GameObject的边界
#### Draw Raycast
- 绘制所有勾选了Is RayCast的组件的边界
#### 显示预览效果
- 生成预览图
#### 关闭预览效果
- 关闭预览图
#### 导出图片
- 将PSD中图片资源导出到导出文件夹中
#### 生成UI界面
- 使用导出文件夹中的图片生成UI界面
#### 清空
- 将当前GameObject下所有子GameObject删除

### Psd 2 UI Element组件
#### Type
- 表明当前GameObject对应的类型
#### Link Psd
- Psd修改时是否进行同步
