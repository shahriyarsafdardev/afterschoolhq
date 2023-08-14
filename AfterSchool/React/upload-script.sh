
source ./.env

echo "Uploading Source Maps to Rollbar..."
for base_url in "${BASE_URLS[@]}"; do
  echo "Uploading Source Maps for $base_url"
  for filename in ./sourceMaps/*; do
    sliced=${filename//.\/sourceMaps/""}
    without_map=${sliced//.map/""}
    minified_url="$base_url/React/dist$without_map"
    source_map="./${filename//.\//""}"

      curl --request POST \
          --url https://api.rollbar.com/api/1/sourcemap \
          --header 'content-type: multipart/form-data'\
          -F access_token=$ROLLBAR_SERVER_KEY \
          -F version="$CODE_VERSION" \
          -F minified_url="$minified_url" \
          -F source_map=@$source_map
  done
done

echo "Source Maps uploaded to Rollbar successfully!"


rm -rf sourceMaps
echo "Source Maps directory deleted successfully"
