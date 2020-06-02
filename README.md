# PSD导入为UGUI工具使用文档


----
## 介绍
- 将PSD文件转换为Unity中的UGUI

## 文档
[美术]()  
[程序]()

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
