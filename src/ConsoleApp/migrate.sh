#!/bin/bash

set -ex

dotnet run -- migrate --data-lake

<<<<<<< HEAD
dotnet run -- migrate --application

=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
dotnet run -- init-facebook-lake
