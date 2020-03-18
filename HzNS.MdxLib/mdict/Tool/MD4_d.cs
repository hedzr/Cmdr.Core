namespace HzNS.MdxLib.MDict.Tool
{
    /// <summary>
    /// mdx/mdd 专用版本
    /// </summary>
    public sealed class MD4_d
    {
        private static uint a(uint i, uint j)
        {
            uint l = i >> (int) (32 - j) | (i << (int) j);
            return l;
        }

        private static uint a(uint i, uint j, uint k)
        {
            uint l = ~i & k;
            uint i1 = j & i;
            return l | i1;
        }

        private static uint a(uint i, uint j, uint k, uint l, uint i1, uint j1)
        {
            return a(a(j, k, l) + i1 + 0x5a827999 + i, j1);
        }

        //public static byte[] mdfour(byte in[]) {
        //    System.out.printf("mdfour(): in.length = %d\n", in.length);
        //    /*
        //     * uint[] ai = new uint[(in.length + 3) / 4]; for (uint i = 0, k = 0; i <
        //     * ai.length; i++) { uint x = 0; if (k < in.length) x |= (in[k++] &
        //     * 0x00ff) << 24; if (k < in.length) x |= (in[k++] & 0x00ff) << 16; if
        //     * (k < in.length) x |= (in[k++] & 0x00ff) << 8; if (k < in.length) x |=
        //     * (in[k++] & 0x00ff); ai[i] = x; }
        //     */
        //    uint[] ai = new uint[16];
        //    uint n = (in[3] & 0xFF) << 24;
        //    uint i1 = (in[2] & 0xFF) << 16;
        //    uint i2 = n | i1;
        //    uint i3 = (in[1] & 0xFF) << 8;
        //    uint i4 = i2 | i3;
        //    uint i5 = in[0] & 0xFF;
        //    uint i6 = i4 | i5;
        //    ai[0] = i6;
        //    ai[1] = 13973;// 0x00003695
        //    ai[2] = 128;// 0x00000080
        //    ai[14] = 64;// 0x00000040
        //    // for (uint i = 0; i < in.length; i++)
        //    // ai[i] = in[i] & 0xff;
        //    // System.out.printf("mdfour(): ai.length = %d\n", ai.length);
        //    uint[] ret = mdfour_i(ai);
        //    /*
        //     * byte[] r = new byte[ret.length * 4]; for (uint i = 0, k = 0; i <
        //     * ret.length; i++) { uint x = ret[i]; r[k++] = (byte) ((x >> 24) &
        //     * 0xff); r[k++] = (byte) ((x >> 16) & 0xff); r[k++] = (byte) ((x >> 8)
        //     * & 0xff); r[k++] = (byte) ((x) & 0xff); }
        //     */
        //    byte[] r = new byte[ret.length * 4];
        //    for (uint i = 0, k = 0; i < ret.length; i++) {
        //        r[k++] = (byte) (ret[i] & 0xff);
        //        r[k++] = (byte) (ret[i] >> 8 & 0xff);
        //        r[k++] = (byte) (ret[i] >> 16 & 0xff);
        //        r[k++] = (byte) (ret[i] >> 24 & 0xff);
        //        System.out.printf("0x%02X, ", r[i]);
        //    }
        //    System.out.println();
        //    return r;
        //}

        public static uint[] mdfour_i(uint[] ai)
        {
            uint[] ai1 = new uint[4];
            uint i = ai[0];
            uint j = d((uint) 0x67452301, (uint) 0xefcdab89, (uint) 0x98badcfe, (uint) 0x10325476, i, 11);
            uint k = ai[1];
            uint l = d((uint) 0x10325476, (uint) j, (uint) 0xefcdab89, (uint) 0x98badcfe, k, 14);
            uint i1 = ai[2];
            uint j1 = j;
            uint k1 = d((uint) 0x98badcfe, (uint) l, (uint) j1, (uint) 0xefcdab89, i1, 15);
            uint l1 = ai[3];
            uint i2 = l;
            uint j2 = j;
            uint k2 = d((uint) 0xefcdab89, (uint) k1, (uint) i2, (uint) j2, l1, 12);
            uint l2 = ai[4];
            uint i3 = j;
            uint j3 = k1;
            uint k3 = l;
            uint l3 = d((uint) i3, (uint) k2, (uint) j3, (uint) k3, l2, 5);
            uint i4 = ai[5];
            uint j4 = l;
            uint k4 = k2;
            uint l4 = k1;
            uint i5 = d((uint) j4, (uint) l3, (uint) k4, (uint) l4, i4, 8);
            uint j5 = ai[6];
            uint k5 = k1;
            uint l5 = l3;
            uint i6 = k2;
            uint j6 = d((uint) k5, (uint) i5, (uint) l5, (uint) i6, j5, 7);
            uint k6 = ai[7];
            uint l6 = k2;
            uint i7 = i5;
            uint j7 = l3;
            uint k7 = d((uint) l6, (uint) j6, (uint) i7, (uint) j7, k6, 9);
            uint l7 = ai[8];
            uint i8 = l3;
            uint j8 = j6;
            uint k8 = i5;
            uint l8 = d((uint) i8, (uint) k7, (uint) j8, (uint) k8, l7, 11);
            uint i9 = ai[9];
            uint j9 = k7;
            uint k9 = j6;
            uint l9 = d((uint) i5, (uint) l8, (uint) j9, (uint) k9, i9, 13);
            uint i10 = ai[10];
            uint j10 = j6;
            uint k10 = k7;
            uint l10 = d((uint) j10, (uint) l9, (uint) l8, (uint) k10, i10, 14);
            uint i11 = ai[11];
            uint j11 = d((uint) k7, (uint) l10, (uint) l9, (uint) l8, i11, 15);
            uint k11 = ai[12];
            uint l11 = l10;
            uint i12 = l9;
            uint j12 = d((uint) l8, (uint) j11, (uint) l11, (uint) i12, k11, 6);
            uint k12 = ai[13];
            uint l12 = l10;
            uint i13 = d((uint) l9, (uint) j12, (uint) j11, (uint) l12, k12, 7);
            uint j13 = ai[14];
            uint k13 = d((uint) l10, (uint) i13, (uint) j12, (uint) j11, j13, 9);
            uint l13 = ai[15];
            uint i14 = i13;
            uint j14 = j12;
            uint k14 = d((uint) j11, (uint) k13, (uint) i14, (uint) j14, l13, 8);
            uint l14 = ai[7];
            uint i15 = i13;
            uint j15 = a(j12, k14, k13, i15, l14, 7);
            uint k15 = ai[4];
            uint l15 = a(i13, j15, k14, k13, k15, 6);
            uint i16 = ai[13];
            uint j16 = j15;
            uint k16 = k14;
            uint l16 = a(k13, l15, j16, k16, i16, 8);
            uint i17 = ai[1];
            uint j17 = j15;
            uint k17 = a(k14, l16, l15, j17, i17, 13);
            uint l17 = ai[10];
            uint i18 = a(j15, k17, l16, l15, l17, 11);
            uint j18 = ai[6];
            uint k18 = k17;
            uint l18 = l16;
            uint i19 = a(l15, i18, k18, l18, j18, 9);
            uint j19 = ai[15];
            uint k19 = k17;
            uint l19 = a(l16, i19, i18, k19, j19, 7);
            uint i20 = ai[3];
            uint j20 = a(k17, l19, i19, i18, i20, 15);
            uint k20 = ai[12];
            uint l20 = l19;
            uint i21 = i19;
            uint j21 = a(i18, j20, l20, i21, k20, 7);
            uint k21 = ai[0];
            uint l21 = l19;
            uint i22 = a(i19, j21, j20, l21, k21, 12);
            uint j22 = ai[9];
            uint k22 = a(l19, i22, j21, j20, j22, 15);
            uint l22 = ai[5];
            uint i23 = i22;
            uint j23 = j21;
            uint k23 = a(j20, k22, i23, j23, l22, 9);
            uint l23 = ai[2];
            uint i24 = i22;
            uint j24 = a(j21, k23, k22, i24, l23, 11);
            uint k24 = ai[14];
            uint l24 = a(i22, j24, k23, k22, k24, 7);
            uint i25 = ai[11];
            uint j25 = j24;
            uint k25 = k23;
            uint l25 = a(k22, l24, j25, k25, i25, 13);
            uint i26 = ai[8];
            uint j26 = j24;
            uint k26 = a(k23, l25, l24, j26, i26, 12);
            uint l26 = ai[3];
            uint i27 = b(j24, k26, l25, l24, l26, 11);
            uint j27 = ai[10];
            uint k27 = k26;
            uint l27 = l25;
            uint i28 = b(l24, i27, k27, l27, j27, 13);
            uint j28 = ai[14];
            uint k28 = k26;
            uint l28 = b(l25, i28, i27, k28, j28, 6);
            uint i29 = ai[4];
            uint j29 = b(k26, l28, i28, i27, i29, 7);
            uint k29 = ai[9];
            uint l29 = l28;
            uint i30 = i28;
            uint j30 = b(i27, j29, l29, i30, k29, 14);
            uint k30 = ai[15];
            uint l30 = l28;
            uint i31 = b(i28, j30, j29, l30, k30, 9);
            uint j31 = ai[8];
            uint k31 = b(l28, i31, j30, j29, j31, 13);
            uint l31 = ai[1];
            uint i32 = i31;
            uint j32 = j30;
            uint k32 = b(j29, k31, i32, j32, l31, 15);
            uint l32 = ai[2];
            uint i33 = i31;
            uint j33 = b(j30, k32, k31, i33, l32, 14);
            uint k33 = ai[7];
            uint l33 = b(i31, j33, k32, k31, k33, 8);
            uint i34 = ai[0];
            uint j34 = j33;
            uint k34 = k32;
            uint l34 = b(k31, l33, j34, k34, i34, 13);
            uint i35 = ai[6];
            uint j35 = j33;
            uint k35 = b(k32, l34, l33, j35, i35, 6);
            uint l35 = ai[13];
            uint i36 = b(j33, k35, l34, l33, l35, 5);
            uint j36 = ai[11];
            uint k36 = k35;
            uint l36 = l34;
            uint i37 = b(l33, i36, k36, l36, j36, 12);
            uint j37 = ai[5];
            uint k37 = k35;
            uint l37 = b(l34, i37, i36, k37, j37, 7);
            uint i38 = ai[12];
            uint j38 = b(k35, l37, i37, i36, i38, 5);
            uint k38 = ai[1];
            uint l38 = l37;
            uint i39 = i37;
            uint j39 = c(i36, j38, l38, i39, k38, 11);
            uint k39 = ai[9];
            uint l39 = l37;
            uint i40 = c(i37, j39, j38, l39, k39, 12);
            uint j40 = ai[11];
            uint k40 = c(l37, i40, j39, j38, j40, 14);
            uint l40 = ai[10];
            uint i41 = i40;
            uint j41 = j39;
            uint k41 = c(j38, k40, i41, j41, l40, 15);
            uint l41 = ai[0];
            uint i42 = i40;
            uint j42 = c(j39, k41, k40, i42, l41, 14);
            uint k42 = ai[8];
            uint l42 = c(i40, j42, k41, k40, k42, 15);
            uint i43 = ai[12];
            uint j43 = j42;
            uint k43 = k41;
            uint l43 = c(k40, l42, j43, k43, i43, 9);
            uint i44 = ai[4];
            uint j44 = j42;
            uint k44 = c(k41, l43, l42, j44, i44, 8);
            uint l44 = ai[13];
            uint i45 = c(j42, k44, l43, l42, l44, 9);
            uint j45 = ai[3];
            uint k45 = k44;
            uint l45 = l43;
            uint i46 = c(l42, i45, k45, l45, j45, 14);
            uint j46 = ai[7];
            uint k46 = k44;
            uint l46 = c(l43, i46, i45, k46, j46, 5);
            uint i47 = ai[15];
            uint j47 = c(k44, l46, i46, i45, i47, 6);
            uint k47 = ai[14];
            uint l47 = l46;
            uint i48 = i46;
            uint j48 = c(i45, j47, l47, i48, k47, 8);
            uint k48 = ai[5];
            uint l48 = l46;
            uint i49 = c(i46, j48, j47, l48, k48, 6);
            uint j49 = ai[6];
            uint k49 = c(l46, i49, j48, j47, j49, 5);
            uint l49 = ai[2];
            uint i50 = i49;
            uint j50 = j48;
            uint k50 = c(j47, k49, i50, j50, l49, 12);
            uint l50 = ai[5];
            uint i51 = g((uint) 0x67452301, (uint) 0xefcdab89, (uint) 0x98badcfe, (uint) 0x10325476, l50, 8);
            uint j51 = ai[14];
            uint k51 = g((uint) 0x10325476, (uint) i51, (uint) 0xefcdab89, (uint) 0x98badcfe, j51, 9);
            uint l51 = ai[7];
            uint i52 = i51;
            uint j52 = g((uint) 0x98badcfe, (uint) k51, (uint) i52, (uint) 0xefcdab89, l51, 9);
            uint k52 = ai[0];
            uint l52 = k51;
            uint i53 = i51;
            uint j53 = g((uint) 0xefcdab89, (uint) j52, (uint) l52, (uint) i53, k52, 11);
            uint k53 = ai[9];
            uint l53 = i51;
            uint i54 = j52;
            uint j54 = k51;
            uint k54 = g((uint) l53, (uint) j53, (uint) i54, (uint) j54, k53, 13);
            uint l54 = ai[2];
            uint i55 = k51;
            uint j55 = j53;
            uint k55 = j52;
            uint l55 = g((uint) i55, (uint) k54, (uint) j55, (uint) k55, l54, 15);
            uint i56 = ai[11];
            uint j56 = j52;
            uint k56 = k54;
            uint l56 = j53;
            uint i57 = g((uint) j56, (uint) l55, (uint) k56, (uint) l56, i56, 15);
            uint j57 = ai[4];
            uint k57 = j53;
            uint l57 = l55;
            uint i58 = k54;
            uint j58 = g((uint) k57, (uint) i57, (uint) l57, (uint) i58, j57, 5);
            uint k58 = ai[13];
            uint l58 = k54;
            uint i59 = i57;
            uint j59 = l55;
            uint k59 = g((uint) l58, (uint) j58, (uint) i59, (uint) j59, k58, 7);
            uint l59 = ai[6];
            uint i60 = j58;
            uint j60 = i57;
            uint k60 = g((uint) l55, (uint) k59, (uint) i60, (uint) j60, l59, 7);
            uint l60 = ai[15];
            uint i61 = i57;
            uint j61 = j58;
            uint k61 = g((uint) i61, (uint) k60, (uint) k59, (uint) j61, l60, 8);
            uint l61 = ai[8];
            uint i62 = g((uint) j58, (uint) k61, (uint) k60, (uint) k59, l61, 11);
            uint j62 = ai[1];
            uint k62 = k61;
            uint l62 = k60;
            uint i63 = g((uint) k59, (uint) i62, (uint) k62, (uint) l62, j62, 14);
            uint j63 = ai[10];
            uint k63 = k61;
            uint l63 = g((uint) k60, (uint) i63, (uint) i62, (uint) k63, j63, 14);
            uint i64 = ai[3];
            uint j64 = g((uint) k61, (uint) l63, (uint) i63, (uint) i62, i64, 12);
            uint k64 = ai[12];
            uint l64 = l63;
            uint i65 = i63;
            uint j65 = g((uint) i62, (uint) j64, (uint) l64, (uint) i65, k64, 6);
            uint k65 = ai[6];
            uint l65 = l63;
            uint i66 = f(i63, j65, j64, l65, k65, 9);
            uint j66 = ai[11];
            uint k66 = f(l63, i66, j65, j64, j66, 13);
            uint l66 = ai[3];
            uint i67 = i66;
            uint j67 = j65;
            uint k67 = f(j64, k66, i67, j67, l66, 15);
            uint l67 = ai[7];
            uint i68 = i66;
            uint j68 = f(j65, k67, k66, i68, l67, 7);
            uint k68 = ai[0];
            uint l68 = f(i66, j68, k67, k66, k68, 12);
            uint i69 = ai[13];
            uint j69 = j68;
            uint k69 = k67;
            uint l69 = f(k66, l68, j69, k69, i69, 8);
            uint i70 = ai[5];
            uint j70 = j68;
            uint k70 = f(k67, l69, l68, j70, i70, 9);
            uint l70 = ai[10];
            uint i71 = f(j68, k70, l69, l68, l70, 11);
            uint j71 = ai[14];
            uint k71 = k70;
            uint l71 = l69;
            uint i72 = f(l68, i71, k71, l71, j71, 7);
            uint j72 = ai[15];
            uint k72 = k70;
            uint l72 = f(l69, i72, i71, k72, j72, 7);
            uint i73 = ai[8];
            uint j73 = f(k70, l72, i72, i71, i73, 12);
            uint k73 = ai[12];
            uint l73 = l72;
            uint i74 = i72;
            uint j74 = f(i71, j73, l73, i74, k73, 7);
            uint k74 = ai[4];
            uint l74 = l72;
            uint i75 = f(i72, j74, j73, l74, k74, 6);
            uint j75 = ai[9];
            uint k75 = f(l72, i75, j74, j73, j75, 15);
            uint l75 = ai[1];
            uint i76 = i75;
            uint j76 = j74;
            uint k76 = f(j73, k75, i76, j76, l75, 13);
            uint l76 = ai[2];
            uint i77 = i75;
            uint j77 = f(j74, k76, k75, i77, l76, 11);
            uint k77 = ai[15];
            uint l77 = e(i75, j77, k76, k75, k77, 9);
            uint i78 = ai[5];
            uint j78 = j77;
            uint k78 = k76;
            uint l78 = e(k75, l77, j78, k78, i78, 7);
            uint i79 = ai[1];
            uint j79 = j77;
            uint k79 = e(k76, l78, l77, j79, i79, 15);
            uint l79 = ai[3];
            uint i80 = e(j77, k79, l78, l77, l79, 11);
            uint j80 = ai[7];
            uint k80 = k79;
            uint l80 = l78;
            uint i81 = e(l77, i80, k80, l80, j80, 8);
            uint j81 = ai[14];
            uint k81 = k79;
            uint l81 = e(l78, i81, i80, k81, j81, 6);
            uint i82 = ai[6];
            uint j82 = e(k79, l81, i81, i80, i82, 6);
            uint k82 = ai[9];
            uint l82 = l81;
            uint i83 = i81;
            uint j83 = e(i80, j82, l82, i83, k82, 14);
            uint k83 = ai[11];
            uint l83 = l81;
            uint i84 = e(i81, j83, j82, l83, k83, 12);
            uint j84 = ai[8];
            uint k84 = e(l81, i84, j83, j82, j84, 13);
            uint l84 = ai[12];
            uint i85 = j82;
            uint j85 = i84;
            uint k85 = j83;
            uint l85 = e(i85, k84, j85, k85, l84, 5);
            uint i86 = ai[2];
            uint j86 = k84;
            uint k86 = i84;
            uint l86 = e(j83, l85, j86, k86, i86, 14);
            uint i87 = ai[10];
            uint j87 = k84;
            uint k87 = e(i84, l86, l85, j87, i87, 13);
            uint l87 = ai[0];
            uint i88 = e(k84, k87, l86, l85, l87, 13);
            uint j88 = ai[4];
            uint k88 = l85;
            uint l88 = k87;
            uint i89 = l86;
            uint j89 = e(k88, i88, l88, i89, j88, 7);
            uint k89 = ai[13];
            uint l89 = i88;
            uint i90 = k87;
            uint j90 = e(l86, j89, l89, i90, k89, 5);
            uint k90 = ai[8];
            uint l90 = i88;
            uint i91 = d((uint) k87, (uint) j90, (uint) j89, (uint) l90, k90, 15);
            uint j91 = ai[6];
            uint k91 = d((uint) i88, (uint) i91, (uint) j90, (uint) j89, j91, 5);
            uint l91 = ai[4];
            uint i92 = j89;
            uint j92 = i91;
            uint k92 = j90;
            uint l92 = d((uint) i92, (uint) k91, (uint) j92, (uint) k92, l91, 8);
            uint i93 = ai[1];
            uint j93 = k91;
            uint k93 = i91;
            uint l93 = d((uint) j90, (uint) l92, (uint) j93, (uint) k93, i93, 11);
            uint i94 = ai[3];
            uint j94 = k91;
            uint k94 = d((uint) i91, (uint) l93, (uint) l92, (uint) j94, i94, 14);
            uint l94 = ai[11];
            uint i95 = d((uint) k91, (uint) k94, (uint) l93, (uint) l92, l94, 14);
            uint j95 = ai[15];
            uint k95 = l92;
            uint l95 = k94;
            uint i96 = l93;
            uint j96 = d((uint) k95, (uint) i95, (uint) l95, (uint) i96, j95, 6);
            uint k96 = ai[0];
            uint l96 = i95;
            uint i97 = k94;
            uint j97 = d((uint) l93, (uint) j96, (uint) l96, (uint) i97, k96, 14);
            uint k97 = ai[5];
            uint l97 = i95;
            uint i98 = d((uint) k94, (uint) j97, (uint) j96, (uint) l97, k97, 6);
            uint j98 = ai[12];
            uint k98 = d((uint) i95, (uint) i98, (uint) j97, (uint) j96, j98, 9);
            uint l98 = ai[2];
            uint i99 = j96;
            uint j99 = i98;
            uint k99 = j97;
            uint l99 = d((uint) i99, (uint) k98, (uint) j99, (uint) k99, l98, 12);
            uint i100 = ai[13];
            uint j100 = k98;
            uint k100 = i98;
            uint l100 = d((uint) j97, (uint) l99, (uint) j100, (uint) k100, i100, 9);
            uint i101 = ai[9];
            uint j101 = k98;
            uint k101 = d((uint) i98, (uint) l100, (uint) l99, (uint) j101, i101, 12);
            uint l101 = ai[7];
            uint i102 = d((uint) k98, (uint) k101, (uint) l100, (uint) l99, l101, 5);
            uint j102 = ai[10];
            uint k102 = l99;
            uint l102 = k101;
            uint i103 = l100;
            uint j103 = d((uint) k102, (uint) i102, (uint) l102, (uint) i103, j102, 15);
            uint k103 = ai[14];
            uint l103 = i102;
            uint i104 = k101;
            uint j104 = d((uint) l100, (uint) j103, (uint) l103, (uint) i104, k103, 8);
            uint k104 = (uint) (0xefcdab89 + k49 + i102);
            uint l104 = (uint) (k101 + i49 + 0x98badcfe);
            ai1[1] = l104;
            uint i105 = j104 + j48 + 0x10325476;
            ai1[2] = i105;
            uint j105 = j103 + k50 + 0x67452301;
            ai1[3] = j105;
            ai1[0] = k104;
            return ai1;
        }

        private static uint b(uint i, uint j, uint k)
        {
            uint l = ~k & j;
            uint i1 = k & i;
            return l | i1;
        }

        private static uint b(uint i, uint j, uint k, uint l, uint i1, uint j1)
        {
            return a(((~k | j) ^ l) + i1 + 0x6ed9eba1 + i, j1);
        }

        private static uint c(uint i, uint j, uint k, uint l, uint i1, uint j1)
        {
            return a((b(j, k, l) + i1 + i) - 0x70e44324, j1);
        }

        private static uint d(uint i, uint j, uint k, uint l, uint i1, uint j1)
        {
            return a(((uint) (l ^ k ^ j)) + i1 + (uint) i, j1);
        }

        private static uint e(uint i, uint j, uint k, uint l, uint i1, uint j1)
        {
            return a(a(j, k, l) + i1 + 0x6d703ef3 + i, j1);
        }

        private static uint f(uint i, uint j, uint k, uint l, uint i1, uint j1)
        {
            return a(((~k | j) ^ l) + i1 + 0x5c4dd124 + i, j1);
        }

        private static uint g(uint i, uint j, uint k, uint l, uint i1, uint j1)
        {
            return a(b((uint) j, (uint) k, (uint) l) + i1 + 0x50a28be6 + (uint) i, j1);
        }
    }
}