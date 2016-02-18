# Unity-ToDo
Simple tool for managing todo`s in your code

![screenshot](http://i.imgur.com/9ZtQP9M.png)

Features
--------

- Parses your code to make list of tasks
- Cusomizable tags
- Refreshes list on file change
- Dounble click on todo to open it 
- Search

How to use
----------
Simply drag ToDo folder to your project and open Tools > Todo
If you want to use data path different from "Assets/Todo/todo.asset" change this line in ToDoEditor.cs

```C#
private string _dataPath = @"Assets/ToDo/todo.asset";
```
