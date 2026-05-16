# TestConsole Sample Application

## Overview

This sample application demonstrates the **OneImlx.Terminal** framework, showcasing **Declarative Multi-Run Methods** with CompositeGroups and traditional IsolatedGroups.

## Command Hierarchy

```
test (Root)
├── grp1 (CompositeGroup - run methods)
│   ├── cmd1 (run method)
│   ├── cmd2 (run method)
│   ├── cmd3 (run method)
│   └── grp2 (CompositeGroup - run methods, nested under grp1)
│       ├── cmd4 (run method)
│       ├── cmd5 (run method)
│       └── cmd6 (run method)
│
└── grp3 (IsolatedGroup - INDEPENDENT, traditional separate classes)
    ├── cmd7 (Cmd7Runner class)
    ├── cmd8 (Cmd8Runner class)
    └── cmd9 (Cmd9Runner class with custom checker)
```

### Key Points

- **grp1**: CompositeGroup with `cmd1`, `cmd2`, `cmd3` as run methods
- **grp2**: Nested CompositeGroup under grp1 with `cmd4`, `cmd5`, `cmd6` as run methods
- **grp3**: Independent IsolatedGroup with `cmd7`, `cmd8`, `cmd9` as separate runner classes
