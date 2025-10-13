const ISSUE_TYPES = [
  {
    "id": "feature",
    "name": "Feature",
    "color": "#2f80ed"
  },
  {
    "id": "bug",
    "name": "Bug",
    "color": "#e55353"
  },
  {
    "id": "improvement",
    "name": "Improvement",
    "color": "#56cc9d"
  }
];
const STATUS_TYPES = [
  {
    "id": "todo",
    "name": "To Do",
    "color": "#b0b0b0"
  },
  {
    "id": "in-progress",
    "name": "In Progress",
    "color": "#f0c419"
  },
  {
    "id": "review",
    "name": "Review",
    "color": "#2fafed"
  },
  {
    "id": "done",
    "name": "Done",
    "color": "#56cc9d"
  },
  {
    "id": "blocked",
    "name": "Blocked",
    "color": "#e55353"
  }
];
const DATA = {
  "issues": [
    {
      "id": "issue_1",
      "title": "Create player movement",
      "link": "TODO",
      "status": "todo",
      "type": "feature",
      "column": 1,
      "row": 26
    },
    {
      "id": "issue_2",
      "title": "Create player shooting",
      "link": "TODO",
      "status": "todo",
      "type": "feature",
      "column": 1,
      "row": 339
    },
    {
      "id": "issue_3",
      "title": "Create enemy movement",
      "link": "TODO",
      "status": "todo",
      "type": "feature",
      "column": 1,
      "row": 180
    },
    {
      "id": "issue_4",
      "title": "Create abstract movement class",
      "link": "TODO",
      "status": "todo",
      "type": "feature",
      "column": 0,
      "row": 59
    },
    {
      "id": "issue_5",
      "title": "Create target follower",
      "link": "TODO",
      "status": "todo",
      "type": "feature",
      "column": 0,
      "row": 627
    },
    {
      "id": "issue_6",
      "title": "Create basic function with renderer",
      "link": "TODO",
      "status": "todo",
      "type": "feature",
      "column": 0,
      "row": 317
    }
  ],
  "links": [
    [
      "issue_4",
      "issue_1"
    ],
    [
      "issue_4",
      "issue_3"
    ],
    [
      "issue_6",
      "issue_2"
    ]
  ]
};