#!/bin/bash

printf \
'//-------------------------------------------------
//     This is a generated file. Do not edit!
//-------------------------------------------------
' > HarmonyPacked.cs

FILES=`find ../Harmony -iname "*.cs" | egrep -i -v '\./obj/|AssemblyInfo\.cs'` 
awk 'FNR==1 {
	print ""
	print "//", substr(FILENAME, index(FILENAME, "/") + 1)
	print ""
}
{
	print
}
' $FILES >> HarmonyPacked.cs

