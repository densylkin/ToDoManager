# Unity-ToDo
Simple tool for managing todo`s in your code

#Features
- Parses your code to make list of tasks
- Cusomizable tags
- Refreshes list on file change
- Dounble click on todo to open it 
- Search

![screenshot](http://i.imgur.com/9ZtQP9M.png)

#How to use
This extentions parses you code to build task list so you need to use this syntax in your scripts:
//YOURTAG YOURNOTE
Something like "//BUG Incorrect position of new objects"
There are two default tags TODO and BUG, but also you can add your own. To do this enter new tag name in field at the bottom of left panel and press add button. 
To open script on line where note is double click on todo in list.


If you want to use data path different from "Assets/Todo/todo.asset" change this line in ToDoEditor.cs

```C#
private string _dataPath = @"Assets/ToDo/todo.asset";
```

