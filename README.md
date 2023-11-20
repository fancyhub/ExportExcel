# ExportExcel
support C#, Go, Cpp, Lua  
Support Csv, Bin

# 数据表   
## 概述
如果数据表要分表, 以第一个读取的表格为主, 后面的表格只能 少列, 移动列, 不能少列,否则会报错  

样例
<table>
  <tr>
    <td bgcolor="green"> 字段名</td>
    <td>Id</td>
    <td>FieldName1</td>
    <td>#Comment (以#开头,这列不读取)</td>
    <td>Value</td>
  </tr>
  <tr>
    <td bgcolor="green">类型和约束 用 "|" 或者 "换行" 分割</td>
    <td>int<br/>PK</td>
    <td>int64<br/>Unique|Export[Client]</td>
    <td></td>
    <td>int</td>
  </tr>
  <tr>
    <td bgcolor="green">描述</td>
    <td>Id描述</td>
    <td>FieldName1 描述</td>
    <td></td>
    <td>数值</td>
  </tr>    
  <tr>
    <td bgcolor="green">从第4行开始下面都是数据区<br/></td>
    <td>#1001 (每行第一列以#开头, 这行不读取)</td>
    <td>物品1</td>
    <td>物品1 的注释</td>
    <td>4</td>
  </tr>    
  <tr>
    <td bgcolor="green">数据区<br/></td>
    <td>1001</td>
    <td>物品2</td>
    <td>物品2 的注释</td>
    <td>40</td>
  </tr>    
</table>

## 表格Sheet名字上的约束
    用 | 分割, 前面的是 sheet name, 第二个是整张表格的导出配置, 有如下这几种, 大小写无所谓
    export_none , export_svr , export_client

    比如 WeaponRandom | Export_Svr         //武器随机表, 服务器需要
    比如 Loc | Export_Client               //多语言表 只有客户端需要
    比如 Item                              //物品表, 都需要


# 类型
## 基础数据类型
| 类型名|描述|
| ---- | ----|
|bool | 可以填写 "YES", "TRUE", "是", "1" 这几个对应 true 其他的全部对应false|
|int/int32, uint/uint32, int64,uint64,float,double| |
|string||
|locstr|会自动生成多语言表, 内容填写 指定默认语言,默认语言在配置里面指定|

## Tuple 类型
Tuple 用 | 作为连接符, 比如 int_float   3|3.5  
所有的类型都可以相互组合, 只有LocStr 不允许和任何其他类型组合  

||bool, int,uint,int64,uint64,float,double,string|locstr|
| ---- | ----|----|
|bool <br> int <br> int32 <br>uint <br>uint32<br>int64<br>uint64<br>float<br>double<br>string|bool_bool<br>int_bool<br>int32_bool<br>bool_string<br>float_float_float_float_float  (最多5个)|不允许|
|locstr|不允许|不允许|


## list类型
list 用 ; 作为连接符, 和 tuple 类似, 任意类型都可以, 就是locstr 不允许  
比如 list_int:  3;4;5  
比如 int_bool:  3:true;4:false

|类型|对应的List类型|
|---|---|
|bool|list_bool|
|int|list_int|
|int64|list_int64|
|float|list_float|
|double|list_double|
|string|list_string|
|locstr|list_locstr|
|x_y (tuple类型)|list_x_y, 比如 list_int_bool|
|x_y_z (tuple类型)|list_x_y_z|

# 约束
 多个约束之间 用 换行 或者 | 分割
 |约束名|描述|
 |----|----|
 |PK <br> PK[sec_key_name] | 主Key,<br>不允许重复, 不允许空,<br>只能支持 int,int64,string 这三种类型,不支持Enum约束,<br>如果有sec_key 为组合key, 目前只支持 int,int的组合|
 |Unique|唯一, <br>不允许重复,不允许空,<br>只能支持 int,int64,string 这三种类型, 不支持Enum约束<br> PK 约束包含了 Unique|
 |Export[] | 控制导出的约束<br>Export[Client]   只导出客户端<br>Export[Svr]   只导出服务器<br>没有填写,默认服务器客户端一起导出<br>如果该字段不想导出, 把该字段的名字改为 #FieldName, 该字段就不再导出了|
 |BlankForbid|不允许为空,<br>标记为PK 或者 Unique的时候, 也包含了 BlankForbid|
 |Enum[Enum_Define_Name] | 枚举类型<br>支持 int 其他类型不支持<br>字段名 可以填写定义的枚举名,也可以填写对应的int值, 如果该int值没有定义,报错|
 |LookUp[TarSheetName.ColName]|查找约束<br>支持 int,uint,list_int,list_uint,int64,uint64, list_int64,string,list_string 不支持枚举<br>注意: TarSheetName.ColName 必须为 BlankForbid / Unique /PK 约束<br>自己字段可以为空<br>中间是点连接符|
 |FilePath[LookUpDir ,Suffix ]|文件路径检查<br>只是支持 string类型, 不支持 list_string<br>比如  FilePath[Assets/Res/Hero,prefab]  该字段的内容填写 file_name<br>工具会检查 Assets/Res/Hero/file_name.prefab 是否存在,同时检查大小写|
 |Range[min,max]|范围检查<br>支持 int32,uint32,int64,uint64, floag,double 以及对应的list 类型|
 |TupleAlias[AliasName]|给Tuple类型增加别名, 目前只有CSharp的导出支持, 对应的别名需要实现对应的 静态方法 CreateInst|  

 # 引用表
表名 @RefTable, 或者 @RefTable_xxx , 可以分表
表结构如下

|该cell留空|SubKeyName1|SubKeyName2|#yyy #开头不读取该列|
|------|------|------|-----|
|MainKeyName1|A|B|x|
|MainKeyName2|C|D||
|#xxx #开头不读取该行|||

||伤害1|
|------|------|
|火|1|
|冰|3|


使用的表 在数据区域里面  
比如填写 R(MainKeyName1, SubKeyName1)  相当于填写 A  
比如填写 R(火,伤害1)  相当于填写 1  

# 枚举表
表名 @EnumConfig  或者 @EnumConfig_xxxx , 可以分表  
样例:
|EnumName|EnumFieldName|ExcelVal|Val|
|------|------|------|-----|
|EItemType|None|无|0|
|EItemType|Weapon|武器|1|
|EItemType|Cosume|消耗品|2|

    第1列: 枚举名
    第2列: 枚举的field name
    第3列: ExcelVal 里面填写的内容, 
    第4列: 对应的值, 只能是int
    
数据表里面:  可以填写 ExcelVal 里面的内容,也可以填写数字  
物品表
|Id|ItemType|
|----|----|
|int\|pk| int \| Enum[EItemType]|
|物品Id|物品类型|
|10001|武器|
|10002|1|
|10003|消耗品|
|10004|2|

# 配置文件 config.json

```json
{
    "excelPaths": [
        "../0_no_loc/data",
        "data"
    ],
    "validation": {
        "searchFileRoot": "../../Client/Resources",
        "sheetNameReg": "^[A-Z][a-zA-Z0-9]*$",
        "colNameReg": "^[A-Z][a-zA-Z0-9_]*$",
        "enumNameReg": "^E[A-Z][A-Za-z0-9_]*$",
        "enumFieldNameReg": "[A-Z][a-zA-Z0-9_]*$",
    },
    "localization": {
		"enable":true,
		"sheetName": "Loc",
		"defaultLang": "zh-Hans",
		"autoGenKey":true,
		"useHashId": true,
    },
	"exportRule":{
		"enable": true,
		"dir": "Output/Rule",
	},
	"exportLocTrans":{
		"enable": true,
		"dir": "Output",
	},
    "exportClient": {
        "csv": {
            "enable": true,
			"utf8bom":true,
            "dir": "Output/Client/Data",
        },
        "bin": {
            "enable": true,
            "dir": "Output/Client/Data",
        },
        "csharp": {
            "enable": true,
            "namespaceName": "Test",
            "classPrefix": "T",
			"classSuffix": "",			
            "dir": "Output/Client/CS",					
            "header": "using System;\n",
			
			"loader":{
				"enable":true,
			},
			"getter":{
				"enable":true,
				"className":"TableMgr",
				"useStatic":false,
			},
			"locId":{
				"enable":true,
				"locIdStartWith":"",
			}
        },	
		"cpp": {
            "enable": true,
            "namespaceName": "Test",
            "classPrefix": "T",
			"classSuffix": "",			
            "dir": "Output/Client/Cpp",					
            "header": "#include \"loc_str.h\"",
			
			"loader":{
				"enable":true,
			},
			"getter":{
				"enable":true,				
			},			 
        },			
        "lua": {
            "enable": true,
            "classPrefix": "T",
			"locIdStartWith": "TC_",
            "dir": "Output/Client/Lua"
        },
    },
    "exportServer": {
        "csv": {
            "enable": true,
			"utf8bom":true,
            "dir": "Output/Server/Data",
        },
        "bin": {
            "enable": true,
            "dir": "Output/Server/Data",
        },
        "csharp": {
           "enable": true,
            "namespaceName": "Test",
            "classPrefix": "T",
			"classSuffix": "",			
            "dir": "Output/Server/CS",					
            "header": "using System;\n",
			
			"loader":{
				"enable":true,
			},
			"getter":{
				"enable":true,
				"className":"TableMgr",
				"useStatic":false,
			},
			"locId":{
				"enable":true,
				"locIdStartWith":"",
			}
        },
        "lua": {
            "enable": true,
            "classPrefix": "T",
			"locIdStartWith": "TC_",
            "dir": "Output/Server/Lua"
        },
        "go": {
            "enable": true,
            "packageName": "config",
            "classPrefix": "T",
            "dir": "Output/Server/Go",
        }
    }
}


```

|字段名|格式要求|描述|
|---|---|---|
|excelPaths|文件/文件夹 列表|是数据 <br/>RefTable 放在这里面|
|validation||是验证相关的配置|
|validation.searchFileRoot|路径|约束FilePath 用的根目录|
 