# PSD导入为UGUI工具使用文档


----
## 介绍
- 将PSD文件转换为Unity中的UGUI

## 文档
[美术](https://github.com/Rising-Moon/PSD2UGUI/blob/develop/Document/%E7%BE%8E%E6%9C%AF%E4%BD%BF%E7%94%A8%E6%96%87%E6%A1%A3.md)  
[程序](https://github.com/Rising-Moon/PSD2UGUI/blob/develop/Document/%E7%A8%8B%E5%BA%8F%E4%BD%BF%E7%94%A8%E6%96%87%E6%A1%A3.md)

## 更新日志
### 0.2
- 修复List只有一行时会出现错误的bug
- 为Psd2UIElement组件添加LinkPsd选项

### 0.3
- 添加了SelectBox类型
- 修改了计算布局的方式，现在根GameObject可以更改锚点  
- 将类型判断及根据类型生成相应Element的方法放到P2UUtil.cs中  
- 更改了匹配规则，现在生成UI后的GameObject名字可以进行更改  
- 剪短了Text类型生成UI时的默认内容

### 0.4
- 调整文档结构
- 将于编辑器相关的代码从PSD2UI.cs中移动到PSD2UIInpector.cs中
