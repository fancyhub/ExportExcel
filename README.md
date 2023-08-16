# ExportExcel
# 数据表   
概述: 如果数据表要分表, 以第一个读取的表格为主, 如果后面同名的表和第一个表的类型不一样,会报错
样例
<table>
  <tr>
    <th bgcolor="green"> 字段名</th>
    <th>Id</th>
    <th>FieldName1</th>
    <th>FieldName2</th>
  </tr>
  <tr>
    <td bgcolor="green">类型和约束 用 "|" 或者 "换行" 分割</td>
    <td>int<br/>PK</td>
    <td>int64<br/>Unique|Export[Client]</td>
    <td>string</td>
  </tr>
  <tr>
    <td bgcolor="green">描述</td>
    <td>Id描述</td>
    <td>FieldName1 描述</td>
    <td>FieldName2 描述</td>
  </tr>  
</table>

## 规则表格 名字上的约束
    用 | 分割, 前面的是 sheet name, 第二个是整张表格的导出配置, 有 export_none, export_svr, export_client 这几种, 大小写无所谓
    比如  WeaponRandom| Export_Svr         //武器随机表, 服务器需要
    比如 Loc | Export_Client                        //多语言表 只有客户端需要
    比如 LocTrans | Export_None               //多语言翻译表不需要导出


# 类型
## 基础数据类型
| 类型名|描述|
| ---- | ----|
|bool | 可以填写 "YES", "TRUE", "是", "1" 这几个对应 true 其他的全部对应false|
|int/int32, uint/uint32, int64,uint64,float,double| |
|string||
|locstr|会自动生成多语言表, 内容填写 指定默认语言,默认语言在配置里面指定|

## pair 类型
pair 用 冒号 | 作为连接符, 比如 int_float   3|3.5
所有的类型都可以相互组合, 只有LocStr 不允许和任何其他类型组合

||bool, int,uint,int64,uint64,float,double,string|locstr|
| ---- | ----|----|
|bool <br> int <br> int32 <br>uint <br>uint32<br>int64<br>uint64<br>float<br>double<br>string|bool_bool<br>int_bool<br>int32_bool<br>bool_string<br>float_float_float_float_float  (最多5个)|不允许|
|locstr|不允许|不允许|


## list类型
list 用 分号 ; 作为连接符, 和pair 类似, 任意类型都可以, 就是locstr 不允许
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
|{x}_{y} (pair类型)|list_{x}_{y}, 比如 list_int_bool|
|{x}_{y}_{z} (pair类型)|list_{x}_{y}_{z}|





