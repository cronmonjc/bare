git log --numstat --since="last sunday" -- *.cs
git log --numstat --pretty="%H" --since="last sunday" -- *.cs | awk 'NF==3 {plus+=$1; minus+=$2} END {printf("total +%d, -%d\n", plus, minus)}'