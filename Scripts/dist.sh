#!/bin/sh

#
# Publish the SensateService backend.
#
# @author Michel Megens
# @email  dev@bietje.net
#

TARGET=$1

dotnet publish -c Release -r $TARGET -o dist
mkdir -p dist
cd dist
ln -s ../Mqtt/dist SensateService.Mqtt
ln -s ../SensateService/dist SensateService

zip -r "SensateService.Mqtt-$TARGET.zip" SensateService.Mqtt
zip -r "SensateService-$TARGET.zip" SensateService

rm -rf ../Core/dist
rm -rf ../Mqtt/dist
rm -rf ../SensateService/dist
rm -rf ../SensateService.Tests/dist
rm -rf SensateService.Mqtt
rm -rf SensateService
