#include <iostream>
#include "amp.h"
#include <chrono>

using namespace concurrency;
using namespace std;
using namespace std::chrono;

void sum_vectors(int* one, int* two, int* result, int size)
{

    array_view<int, 1> o(size, &one[0]);
    array_view<int, 1> t(size, &two[0]);
    array_view<int, 1> res(size, &result[0]);

    parallel_for_each(res.extent, [=](index<1> idx) restrict(amp)
        {
            res[idx] = o[idx] + t[idx];
        });


    res.synchronize();
}

void multiply_matrix_by_number(int* matrix, int number, int* result, int sizeN, int sizeM)
{

    array_view<int, 2> mat(sizeN, sizeM, matrix);
    array_view<int, 2> res(sizeN, sizeM, result);

    parallel_for_each(res.extent, [=](index<2> idx) mutable restrict(amp)
        {
            int num = number;
            res(idx[0], idx[1]) = mat(idx[0], idx[1]) * num;
        });


    res.synchronize();
}

void list_all_accelerators()
{
    std::vector<accelerator> accs = accelerator::get_all();

    for (int i = 0; i < accs.size(); i++) {
        std::wcout << accs[i].get_description() << "\n";
        std::wcout << accs[i].device_path << "\n";
        std::wcout << accs[i].dedicated_memory << "\n";
        std::wcout << (accs[i].supports_cpu_shared_memory ?
            "CPU shared memory: true" : "CPU shared memory: false") << "\n";
        std::wcout << (accs[i].supports_double_precision ?
            "double precision: true" : "double precision: false") << "\n";
        std::wcout << (accs[i].supports_limited_double_precision ?
            "limited double precision: true" : "limited double precision: false") << "\n";
        std::wcout << "\n";
    }
}

void TestSum()
{
    int* one = new int[1000000];
    int* two = new int[1000000];

    std::vector<accelerator> accs = accelerator::get_all();
    accelerator::set_default(accs[0].device_path);

    for (int i = 0; i < 1000000; i++)
    {
        one[i] = i;
        two[i] = i;
    }

    int* res = new int[1000000];

    auto start = high_resolution_clock::now();
    sum_vectors(one, two, res, 1000000);
    auto stop = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(stop - start);
    cout << "Sum vectors; Time: " << duration.count() << " ms" << endl;
}

void TestMatrixNumber()
{
    int* one = new int[1000000];

    std::vector<accelerator> accs = accelerator::get_all();
    accelerator::set_default(accs[0].device_path);

    for (int i = 0; i < 1000000; i++)
    {
        one[i] = i;
    }

    int* res = new int[1000000];

    auto start = high_resolution_clock::now();
    multiply_matrix_by_number(one, 5, res, 1000, 1000);
    auto stop = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(stop - start);
    cout << "Matrix ; Time: " << duration.count() << " ms" << endl;
}

int main()
{
    TestSum();
}
