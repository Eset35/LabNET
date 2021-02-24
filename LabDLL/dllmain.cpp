// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"
#include "amp.h"
#include <string>
#include <iostream>

using namespace concurrency;
using namespace std;


extern "C" __declspec (dllexport) void set_device()
{
    std::vector<accelerator> accs = accelerator::get_all();
    accelerator::set_default(accs[0].device_path);
}

extern "C" __declspec (dllexport) void list_all_accelerators()
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

extern "C" __declspec (dllexport) void _stdcall sum_vectors(int* one, int* two, int* result, int size)
{
    set_device();
    array_view<int, 1> o(size, &one[0]);
    array_view<int, 1> t(size, &two[0]);
    array_view<int, 1> res(size, &result[0]);

    parallel_for_each(res.extent, [=](index<1> idx) restrict(amp)
        {
            res[idx] = o[idx] + t[idx];
        });


    res.synchronize();
}


extern "C" __declspec (dllexport) void _stdcall multiply_matrix_by_number(int* matrix, int number, int* result, int sizeN, int sizeM)
{
    set_device();
    array_view<int, 2> mat(sizeN, sizeM, matrix);
    array_view<int, 2> res(sizeN, sizeM, result);

    parallel_for_each(res.extent, [=](index<2> idx) restrict(amp)
        {
            //int num = number;
            res(idx[0], idx[1]) = mat(idx[0], idx[1]) * 2;
        });


    res.synchronize();
}

extern "C" __declspec (dllexport) void _stdcall trans_matrix(int* matrix_one, int* result, int sizeN, int sizeM)
{
    set_device();
    array_view<int, 2> one(sizeN, sizeM, matrix_one);
    array_view<int, 2> res(sizeM, sizeN, result);

    parallel_for_each(res.extent, [=](index<2> idx) restrict(amp)
        {
            res(idx[0], idx[1]) = one(idx[1], idx[0]);
        });


    res.synchronize();
}

extern "C" __declspec (dllexport) void _stdcall multiply_matrix(int* matrix_one, int* matrix_two, int* size_matrixes, int* result)
{
    set_device();
    array_view<int, 2> a(size_matrixes[0], size_matrixes[1], matrix_one);

    array_view<int, 2> b(size_matrixes[2], size_matrixes[3], matrix_two);

    array_view<int, 2> res(size_matrixes[0], size_matrixes[3], result);

    array_view<int, 1> size(4, size_matrixes);

    parallel_for_each(res.extent,
        [=](index<2> idx) restrict(amp) {
            int row = idx[0];
            int col = idx[1];
            for (int inner = 0; inner < size[2]; inner++) {
                res[idx] += a(row, inner) * b(inner, col);
            }
        });

    res.synchronize();
}





