Framework "4.0"

Properties {
    $build_dir = Split-Path $psake.build_script_file
    $build_artifacts_dir = "$build_dir\build\"
}

task Default -depends Compile

task Compile {    
    Exec { msbuild "solutionizer.sln" /v:quiet /p:OutDir=$build_artifacts_dir }
}