
function PrepareRepo {
	mkdir "_repo_.git"
	cd "_repo_.git"
	git init --bare
	cd ".."

	mkdir _init_
	cd _init_
	git clone ../_repo_.git .
	git config user.name "tester"
	git config user.email "tester@nowhere.com"

	Set-Content -Path file.txt -Value "main-1" -Encoding ascii
	git add -A
	git commit -a -m "1"
	git push

	Add-Content -Path file.txt -Value "main-2" -Encoding ascii
	git add -A
	git commit -a -m "2"
	git push

	$branchPoint = (git rev-parse HEAD)

	Add-Content -Path file.txt -Value "main-3" -Encoding ascii
	git add -A
	git commit -a -m "3"
	git push

	Add-Content -Path file.txt -Value "main-4" -Encoding ascii
	git add -A
	git commit -a -m "4"
	git push

	Add-Content -Path file.txt -Value "main-5" -Encoding ascii
	git add -A
	git commit -a -m "5"
	git push

	git branch work "$branchPoint"
	git checkout work
	git push --set-upstream origin work

	Add-Content -Path file.txt -Value "branch-A" -Encoding ascii
	git add -A
	git commit -a -m "A"
	git push

	Add-Content -Path file.txt -Value "branch-B" -Encoding ascii
	git add -A
	git commit -a -m "B"
	git push

	Add-Content -Path file.txt -Value "branch-C" -Encoding ascii
	git add -A
	git commit -a -m "C"
	git push

	cd ..
}

function MakeClone {
	Param(
		[string]$name,
		[bool]$branch,
		[bool]$prev,
		[bool]$untracked,
		[bool]$noRemote,
		[bool]$ahead,
		[bool]$changed
	)

	mkdir $name
	cd $name

	$b = ""
	if ($branch) { $b = "--branch work" }
	$cmd = "git clone $b `"../_repo_.git`" ."
	echo $cmd
	Invoke-Expression $cmd

	if ($prev) {
		$rev = (git rev-parse "HEAD^1")
		$cmd = "git reset --hard $rev"
		echo $cmd
		Invoke-Expression $cmd
	}

	if ($untracked) {
		git branch --unset-upstream
	}

	if ($noRemote) {
		git remote remove origin
	}

	if ($ahead) {
		Add-Content -Path file.txt -Value "ahead" -Encoding ascii
		git config user.name "tester"
		git config user.email "tester@nowhere.com"
		git add -A
		git commit -a -m "++"
	}

	if ($changed) {
		Add-Content -Path file.txt -Value "changed" -Encoding ascii
	}

	cd ..
}


rm "${PSScriptRoot}\TestData" -recurse -force -ErrorAction Ignore
mkdir "${PSScriptRoot}\TestData"

$env:GIT_REDIRECT_STDERR = '2>&1'

cd "${PSScriptRoot}\TestData"

PrepareRepo

for ($branch = 0; $branch -le 1; $branch++) {
for ($untracked = 0; $untracked -le 1; $untracked++) {
for ($behind = 0; $behind -le 1; $behind++) {
for ($ahead = 0; $ahead -le 1; $ahead++) {
for ($changed = 0; $changed -le 1; $changed++) {
	$n = "c"
	if ($branch) { $n += "-branch" } else { $n += "-main" }
	if ($untracked) { $n += "-untracked" }
	if ($behind) { $n += "-behind" }
	if ($ahead) { $n += "-ahead" }
	if ($changed) { $n += "-changed" }
	MakeClone $n $branch $behind $untracked $false $ahead $changed
}
}
}
}
}

cd "${PSScriptRoot}"
