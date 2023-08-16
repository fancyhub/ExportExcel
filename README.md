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
