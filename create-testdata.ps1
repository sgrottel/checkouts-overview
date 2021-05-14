rm "${PSScriptRoot}\TestData" -recurse -force -ErrorAction Ignore
mkdir "${PSScriptRoot}\TestData"

$env:GIT_REDIRECT_STDERR = '2>&1'

cd "${PSScriptRoot}\TestData"

mkdir "r.git"
cd "r.git"
git init --bare
cd ".."

mkdir i
cd i
git clone ../r.git .
git config user.name "tester"
git config user.email "tester@nowhere.com"
echo "x" > f1.txt
git add -A
git commit -a -m "f1"
git push
cd ..

# will be in
mkdir a
cd a
git clone ../r.git .
git config user.name "tester"
git config user.email "tester@nowhere.com"
cd ..

# will be in&out
mkdir b
cd b
git clone ../r.git .
git config user.name "tester"
git config user.email "tester@nowhere.com"
echo "y" > f2.txt
git add -A
git commit -a -m "f2"
# no push
cd ..

# will be ok
mkdir c
cd c
git clone ../r.git .
git config user.name "tester"
git config user.email "tester@nowhere.com"
echo "w" > f3.txt
git add -A
git commit -a -m "f3"
git push
cd ..

# will be out
mkdir d
cd d
git clone ../r.git .
git config user.name "tester"
git config user.email "tester@nowhere.com"
echo "v" > f4.txt
git add -A
git commit -a -m "f4"
# no push
cd ..

cd a
git fetch
cd ..
cd b
git fetch
cd ..

copy a e -recurse
copy b f -recurse
copy c g -recurse
copy d h -recurse

cd e
echo "changed" > f1.txt
cd ..
cd f
echo "changed" > f1.txt
cd ..
cd g
echo "changed" > f1.txt
cd ..
cd h
echo "changed" > f1.txt
cd ..

cd ..
