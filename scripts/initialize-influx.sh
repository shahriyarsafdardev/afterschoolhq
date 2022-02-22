#!/bin/bash
curl -X POST http://localhost:8086/query --data-urlencode 'q=CREATE DATABASE "local"'
