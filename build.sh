#!/bin/sh



MODNAME="TheInvisibleHand"
MODSRCDIR="$HOME/.local/share/Terraria/ModLoader/Mod Sources/$MODNAME"
CSPROJ="${PROJECTFILE:-$MODNAME.csproj}"

## location of tmodcompiler.exe (or whatever we're using to build with)
TMCDIR="$HOME/.local/share/Steam/steamapps/common/Terraria"
tmcexe="${TMODCOMPILER:-tmodcompiler.exe}"

extra_files=( build.txt description.txt )
debug="${IHDEBUG:-1}"

build_target() {
	## usage: build_target PLATFORM BUILD_TYPE
	## e.g.: build_target Windows Release

	local platform
	local build
	case $1 in
		Windows|windows)
			platform=Windows ;;
		Mono|mono|Linux|linux|mac|Mac)
			platform=Mono ;;
		*)
			echo "$1 is not a recognized build platform" >&2
			return 1 ;;
	esac

	case $2 in
		debug|Debug)
			build=Debug ;;
		Release|release)
			build=Release ;;
		*)
			echo "$2 is not a valid build configuration" >&2
			return 1 ;;
	esac

	#/verbosity:detailed 
	xbuild "/p:Configuration=${platform}${build}" "$CSPROJ" && move_to_modsources "$platform" "$build"
}

move_to_modsources() {
	## usage: move_to_modsources {Windows|Mono} {Debug|Release}

	mv -f "bin/$2/$1/$MODNAME.dll" "$MODSRCDIR/$1.dll"
}

copy_extra_files() {
	cp -f "${extra_files[@]}" "$MODSRCDIR/"
}

tmod_compile() {
	cd "$TMCDIR"

	$tmcexe "$MODSRCDIR"

}

main() {
	mkdir -p "$MODSRCDIR"

	export MONO_IOMAP=all

	local build_type=Release
	(( $debug )) && build_type=Debug

	local errors=0
	for plat in Windows Mono ; do
		build_target $plat $build_type || errors=1
	done

	if (( $errors )); then
		exit 1
	else
		copy_extra_files
		# do this by hand for the moment
		# tmod_compile
	fi


}

main

#
#
# config_type=
# if (( $debug )) ; then
#
# else
# 	config_type=Release
# fi
#
#
#
#
# #xbuild_args="/p:Configuration=$config_type" #/p:Platform=x86
#
# xbuild "/p:Configuration=Windows$config_type" "$csproj" && mv -f "bin/$config_type/Windows/$MODNAME.dll" "$MODSRCDIR/Windows.dll"
#
# xbuild "/p:Configuration=Mono$config_type" "$csproj" && mv -f "bin/$config_type/Mono/$MODNAME.dll" "$MODSRCDIR/Mono.dll"
#
# cp -f build.txt description.txt "$MODSRCDIR/"
