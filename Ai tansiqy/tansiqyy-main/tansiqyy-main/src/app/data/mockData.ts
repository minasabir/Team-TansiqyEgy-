export type UniversityType = 'حكومية' | 'خاصة' | 'أهلية' | 'تكنولوجية' | 'أجنبية' | 'معاهد عليا';

export interface University {
  id: string;
  nameAr: string;
  nameEn: string;
  type: UniversityType;
  location: string;
  image: string;
  logo: string;
  /** غلاف الهوية من شعار الموقع الرسمي بدل صورة توضيحية عشوائية */
  coverMode?: "photo" | "brand";
  /** نبذة عن الجامعة */
  description: string;
  /** موقع الجامعة الرسمي (من الـ API إن وُجد) */
  officialWebsite?: string | null;
}

export interface College {
  id: string;
  universityId: string;
  nameAr: string;
  nameEn: string;
  admissionPercentage: number;
  fees: string;
  description: string;
  departments: string[];
  /** موقع الكلية أو الصفحة الرسمية (من الـ API إن وُجد) */
  officialWebsite?: string | null;
  /** التنسيق مأخوذ من سجل الجامعة لأن الكلية بلا رقم في المنصة */
  coordinationFromUniversity?: boolean;
  /** المصاريف مكمّلة من بيانات الجامعة لأن الكلية بلا تفصيل في المنصة */
  feesFromUniversity?: boolean;
}

export const UNIVERSITIES: University[] = [
  {
    id: '1',
    nameAr: 'جامعة القاهرة',
    nameEn: 'Cairo University',
    type: 'حكومية',
    location: 'الجيزة',
    image: 'https://images.unsplash.com/photo-1760131556605-7f2e63d00385?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx1bml2ZXJzaXR5JTIwY2FtcHVzJTIwbW9kZXJuJTIwYnVpbGRpbmd8ZW58MXx8fHwxNzc0NTMzNDk3fDA&ixlib=rb-4.1.0&q=80&w=1080',
    logo: 'https://images.unsplash.com/photo-1667273704095-66c1e361cfdb?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjb2xsZWdlJTIwbGlicmFyeSUyMGJ1aWxkaW5nfGVufDF8fHx8MTc3NDUzNTc5OHww&ixlib=rb-4.1.0&q=80&w=200',
    description: 'أقدم جامعات مصر والشرق الأوسط، وتضم كليات متنوعة في العلوم الإنسانية والطب والهندسة.',
  },
  {
    id: '2',
    nameAr: 'جامعة عين شمس',
    nameEn: 'Ain Shams University',
    type: 'حكومية',
    location: 'القاهرة',
    image: 'https://images.unsplash.com/photo-1693011142814-aa33d7d1535c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxzdHVkZW50cyUyMGluJTIwdW5pdmVyc2l0eSUyMGNhbXB1c3xlbnwxfHx8fDE3NzQ1MzU3OTh8MA&ixlib=rb-4.1.0&q=80&w=1080',
    logo: 'https://ar.wikipedia.org/wiki/%D8%AC%D8%A7%D9%85%D8%B9%D8%A9_%D8%B9%D9%8A%D9%86_%D8%B4%D9%85%D8%B3',
    description: 'جامعة حكومية كبرى بالقاهرة، معروفة بكليات الطب والهندسة والعلوم القوية.',
  },
  {
    id: '3',
    nameAr: 'الجامعة الألمانية بالقاهرة',
    nameEn: 'German University in Cairo',
    type: 'خاصة',
    location: 'القاهرة الجديدة',
    image: 'https://images.unsplash.com/photo-1773148374151-ba5658056828?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxtb2Rlcm4lMjBlbmdpbmVlcmluZyUyMHVuaXZlcnNpdHl8ZW58MXx8fHwxNzc0NTM1Nzk4fDA&ixlib=rb-4.1.0&q=80&w=1080',
    logo: 'https://images.unsplash.com/photo-1760131556605-7f2e63d00385?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx1bml2ZXJzaXR5JTIwY2FtcHVzJTIwbW9kZXJuJTIwYnVpbGRpbmd8ZW58MXx8fHwxNzc0NTMzNDk3fDA&ixlib=rb-4.1.0&q=80&w=200',
    description: 'جامعة خاصة بشراكات ألمانية، تركز على الهندسة والتكنولوجيا والبرامج الدولية.',
  },
  {
    id: '4',
    nameAr: 'جامعة الملك سلمان الدولية',
    nameEn: 'King Salman International University',
    type: 'أهلية',
    location: 'جنوب سيناء',
    image: 'https://images.unsplash.com/photo-1667273704095-66c1e361cfdb?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjb2xsZWdlJTIwbGlicmFyeSUyMGJ1aWxkaW5nfGVufDF8fHx8MTc3NDUzNTc5OHww&ixlib=rb-4.1.0&q=80&w=1080',
    logo: 'https://images.unsplash.com/photo-1693011142814-aa33d7d1535c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxzdHVkZW50cyUyMGluJTIwdW5pdmVyc2l0eSUyMGNhbXB1c3xlbnwxfHx8fDE3NzQ1MzU3OTh8MA&ixlib=rb-4.1.0&q=80&w=200',
    description: 'جامعة أهلية حديثة بجنوب سيناء تهدف لخدمة التنمية في إقليم قناة السويس.',
  },
  {
    id: '5',
    nameAr: 'جامعة القاهرة الجديدة التكنولوجية',
    nameEn: 'New Cairo Technological University',
    type: 'تكنولوجية',
    location: 'القاهرة الجديدة',
    image: 'https://images.unsplash.com/photo-1773148374151-ba5658056828?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxtb2Rlcm4lMjBlbmdpbmVlcmluZyUyMHVuaXZlcnNpdHl8ZW58MXx8fHwxNzc0NTM1Nzk4fDA&ixlib=rb-4.1.0&q=80&w=1080',
    logo: 'https://images.unsplash.com/photo-1693011142814-aa33d7d1535c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxzdHVkZW50cyUyMGluJTIwdW5pdmVyc2l0eSUyMGNhbXB1c3xlbnwxfHx8fDE3NzQ1MzU3OTh8MA&ixlib=rb-4.1.0&q=80&w=200',
    description: 'جامعة تكنولوجية تركز على البرامج التطبيقية وربط التعليم بسوق العمل.',
  },
  {
    id: '6',
    nameAr: 'الجامعة الأمريكية بالقاهرة',
    nameEn: 'American University in Cairo',
    type: 'أجنبية',
    location: 'القاهرة الجديدة',
    image: 'https://images.unsplash.com/photo-1760131556605-7f2e63d00385?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx1bml2ZXJzaXR5JTIwY2FtcHVzJTIwbW9kZXJuJTIwYnVpbGRpbmd8ZW58MXx8fHwxNzc0NTMzNDk3fDA&ixlib=rb-4.1.0&q=80&w=1080',
    logo: 'https://images.unsplash.com/photo-1667273704095-66c1e361cfdb?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjb2xsZWdlJTIwbGlicmFyeSUyMGJ1aWxkaW5nfGVufDF8fHx8MTc3NDUzNTc5OHww&ixlib=rb-4.1.0&q=80&w=200',
    description: 'جامعة أمريكية معتمدة دولياً، قوية في إدارة الأعمال والعلوم الاجتماعية والفنون.',
  },
  {
    id: '7',
    nameAr: 'المعهد العالي للحاسبات ونظم المعلومات',
    nameEn: 'Higher Institute for Computers',
    type: 'معاهد عليا',
    location: 'التجمع الأول',
    image: 'https://images.unsplash.com/photo-1693011142814-aa33d7d1535c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxzdHVkZW50cyUyMGluJTIwdW5pdmVyc2l0eSUyMGNhbXB1c3xlbnwxfHx8fDE3NzQ1MzU3OTh8MA&ixlib=rb-4.1.0&q=80&w=1080',
    logo: 'https://images.unsplash.com/photo-1760131556605-7f2e63d00385?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx1bml2ZXJzaXR5JTIwY2FtcHVzJTIwbW9kZXJuJTIwYnVpbGRpbmd8ZW58MXx8fHwxNzc0NTMzNDk3fDA&ixlib=rb-4.1.0&q=80&w=200',
    description: 'معهد عالٍ متخصص في الحاسبات ونظم المعلومات والإدارة.',
  },
];

export const COLLEGES: College[] = [
  // Cairo University Colleges
  {
    id: 'c1',
    universityId: '1',
    nameAr: 'كلية الطب القصر العيني',
    nameEn: 'Faculty of Medicine (Kasr Al Ainy)',
    admissionPercentage: 91.6,
    fees: 'مجاني (مصروفات إدارية رمزية)',
    description: 'واحدة من أعرق كليات الطب في الشرق الأوسط وأفريقيا، وتوفر تعليماً طبياً متميزاً.',
    departments: ['الجراحة العامة', 'الباطنة', 'طب الأطفال', 'النساء والتوليد'],
  },
  {
    id: 'c2',
    universityId: '1',
    nameAr: 'كلية الهندسة',
    nameEn: 'Faculty of Engineering',
    admissionPercentage: 85.3,
    fees: 'مجاني (مصروفات إدارية رمزية)',
    description: 'كلية رائدة في مجال العلوم الهندسية والتكنولوجية، تخرج منها آلاف المهندسين المتميزين.',
    departments: ['الهندسة المعمارية', 'الهندسة المدنية', 'هندسة الحاسبات', 'الهندسة الميكانيكية'],
  },
  // GUC Colleges
  {
    id: 'g1',
    universityId: '3',
    nameAr: 'كلية هندسة وتكنولوجيا المعلومات',
    nameEn: 'Faculty of Information Engineering and Technology',
    admissionPercentage: 70.0,
    fees: '١١٠,٠٠٠ ج.م (تقريبي للترم الواحد)',
    description: 'تقدم برامج هندسية متطورة بالتعاون مع الجامعات الألمانية، وتركز على التكنولوجيا الحديثة.',
    departments: ['هندسة الشبكات', 'هندسة الاتصالات', 'هندسة الإلكترونيات'],
  },
  {
    id: 'g2',
    universityId: '3',
    nameAr: 'كلية الصيدلة',
    nameEn: 'Faculty of Pharmacy',
    admissionPercentage: 75.0,
    fees: '١٢٥,٠٠٠ ج.م (تقريبي للترم الواحد)',
    description: 'برنامج صيدلي متميز يجمع بين العلوم الطبية والتطبيقية بمعايير جودة عالية.',
    departments: ['الصيدلانيات', 'الأدوية والسموم', 'الكيمياء الصيدلية'],
  },
  // King Salman
  {
    id: 'k1',
    universityId: '4',
    nameAr: 'كلية علوم الحاسب',
    nameEn: 'Faculty of Computer Science',
    admissionPercentage: 62.0,
    fees: '٦٩,٠٠٠ ج.م (سنوياً)',
    description: 'تؤهل الطلاب لسوق العمل في مجالات البرمجة والذكاء الاصطناعي وعلوم البيانات.',
    departments: ['الذكاء الاصطناعي', 'علوم البيانات', 'هندسة البرمجيات'],
  },
  // Tech University
  {
    id: 't1',
    universityId: '5',
    nameAr: 'كلية تكنولوجيا الصناعة والطاقة',
    nameEn: 'Faculty of Industry and Energy Technology',
    admissionPercentage: 65.0,
    fees: '١٥,٠٠٠ ج.م (سنوياً)',
    description: 'تقدم برامج تكنولوجية حديثة تلبي احتياجات سوق العمل الصناعي بنظام الجدارات.',
    departments: ['تكنولوجيا المعلومات', 'الميكاترونكس', 'الطاقة المتجددة', 'تكنولوجيا الأطراف الصناعية'],
  },
  // AUC
  {
    id: 'a1',
    universityId: '6',
    nameAr: 'كلية إدارة الأعمال',
    nameEn: 'School of Business',
    admissionPercentage: 80.0,
    fees: '٣٥٠,٠٠٠ ج.م (سنوياً)',
    description: 'من أفضل كليات إدارة الأعمال في الشرق الأوسط، حاصلة على الاعتمادات الدولية الثلاثية.',
    departments: ['إدارة الأعمال', 'المحاسبة', 'الاقتصاد', 'التمويل'],
  },
  // Institute
  {
    id: 'i1',
    universityId: '7',
    nameAr: 'شعبة نظم معلومات الأعمال',
    nameEn: 'Business Information Systems Department',
    admissionPercentage: 55.0,
    fees: '١٢,٠٠٠ ج.م (سنوياً)',
    description: 'معهد معتمد من وزارة التعليم العالي يقدم برامج متخصصة في الحاسبات والإدارة.',
    departments: ['نظم المعلومات الإدارية', 'علوم الحاسب', 'المحاسبة'],
  }
];
