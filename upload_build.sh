#!/bin/bash

# Function to display usage information
usage() {
  echo "Usage: $0 <semantic_version_tag>"
  echo "Example: $0 1.0.0"
  exit 1
}

# Check if a version tag is provided
if [ -z "$1" ]; then
  echo "Error: Semantic version tag is required."
  usage
fi

# Semantic version tag
TAG=$1

# Replace dots with dashes in the tag
FILE_FRIENDLY_TAG=${TAG//./-}

ZIP_FILE="ig.zip"

# Create version.txt inside the ig folder
echo "$TAG" > build/ig/version.txt

# Navigate to the build directory and create the zip file
cd build || { echo "Directory build not found."; exit 1; }
zip -r "../$ZIP_FILE" "ig" || { echo "Failed to create zip file."; exit 1; }
cd ..

# Nexus upload URLs
NEXUS_URL_BASE="https://nexus.darkhive.ai/repository/raw-private/image-generator"
NEXUS_URL1="$NEXUS_URL_BASE/$ZIP_FILE"
NEXUS_URL2="$NEXUS_URL_BASE/ig_$FILE_FRIENDLY_TAG.zip"

# Upload files to Nexus
echo "Uploading $ZIP_FILE to $NEXUS_URL1"
curl -v -u "$NEXUS_USERNAME:$NEXUS_PASSWORD" --upload-file "$ZIP_FILE" "$NEXUS_URL1" || { echo "Failed to upload $ZIP_FILE to $NEXUS_URL1."; exit 1; }

# echo "Uploading $ZIP_FILE as ig_$FILE_FRIENDLY_TAG.zip to $NEXUS_URL2"
# curl -v -u "$NEXUS_USERNAME:$NEXUS_PASSWORD" --upload-file "$ZIP_FILE" "$NEXUS_URL2" || { echo "Failed to upload $ZIP_FILE to $NEXUS_URL2."; exit 1; }

echo "Upload completed successfully."
