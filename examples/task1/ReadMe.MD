# Federated Query:
```JSON
{
  "request": {
    "primaryKey": "global_user_id",
    "query": "v2/epampersonpublic/search?q=business_email==podkolzzin1996@gmail.com",
    "parentPath": "",
    "collectionName": "employee",
    "descendants": [
      {
        // primaryKey as jmesPath
        "primaryKey": "objectIds[0].id",
        // {{parents}} array of parent keys to lookup entities from another data set
        "query": "v2/skill-link/search?q=objectIds.id=in=({{parents}})&fields=objectIds.id,spokenLanguages,skillName",
        "parentPath": "employee.global_user_id",
        "collectionName": "spoken_languages"
      }
    ]
  },
  "mapping": "[0].spoken_languages"
}
```

# Flow
1. Request `v2/epampersonpublic/search?q=business_email==podkolzzin1996@gmail.com`
2. Resolve list of IDs from the collection and replace `{{parents}}` placeholder into list of IDs
3. Request `v2/skill-link/search?q=objectIds.id=in=(17, 19)&fields=objectIds.id,spokenLanguages.skillName`
4. Join Collections into a single object
5. Apply mapping
6. Return Result

# Test Data
## Request:
`v2/epampersonpublic/search?q=business_email==podkolzzin1996@gmail.com`
## All Data:
```JSON
[
  {
    "global_user_id": 17,
    "business_email": "podkolzzin1996@gmail.com"
  },
  {
    "global_user_id": 18, 
    "business_email": "podkolzzin1996@gmail.com"
  },
  {
    "global_user_id": 19,
    "business_email": "email4@bla.com",
  }
]
```

## Response:
```JSON
[
  {
    "global_user_id": 17,
    "business_email": "podkolzzin1996@gmail.com"
  },
  {
    "global_user_id": 18, 
    "business_email": "podkolzzin1996@gmail.com"
  }
]
```

## Request
`v2/skill-link/search?q=objectIds.id=in=(17,19)&fields=objectIds.id,spokenLanguages,skillName`
## All Data:
```JSON
[
  {
    "objectIds": [
        {
            "id": 17, // Id is matching filter (17, 19) so the object will be returned
            "name": "not returned" // Some random data to show that is won't be returned
        },
        {
            "id": 24,
            "name": "also not returned"
        }
    ],
    "otherProperty": "not returned",
    "spokenLanguages":  [ "UA", "EN" ],
    "skillName": ".NET"
  },
  { // Object with no matching objectIds.id so it won't be returned
    "objectIds": [ 
        {
            "id": 77,
            "name": "not returned"
        },
        {
            "id": 99,
            "name": "also not returned"
        }
    ],
    "otherProperty": "not returned",
    "spokenLanguages":  [ "UA", "EN" ],
    "skillName": ".NET"
  }
]
```

## Response:
```JSON
[
    {
        "objectIds":[
            {"id": 17 },
            {"id": 24 }
        ],
        "spokenLanguages":  [ "UA", "EN" ],
        "skillName": ".NET"
    }
]
```

# Joined Collections:
```JSON
[
  {
    "17": {
      "_key": "17",
      "employee": {
        "global_user_id": 17,
        "business_email": "podkolzzin1996@gmail.com"
      },
      "spoken_languages": {
        "objectIds": [
          {
            "id": 17
          },
          {
            "id": 24
          }
        ],
        "spokenLanguages": [
          "UA",
          "EN"
        ],
        "skillName": ".NET"
      }
    }
  },
  {
    "19": {
      "_key": "19",
      "employee": {
        "global_user_id": 19,
        "business_email": "podkolzzin1996@gmail.com"
      }
    }
  }
]
```

# Mapped:
```JSON
{
  "objectIds": [
    {
      "id": 17
    },
    {
      "id": 24
    }
  ],
  "spokenLanguages": [
    "UA",
    "EN"
  ],
  "skillName": ".NET"
}
```
