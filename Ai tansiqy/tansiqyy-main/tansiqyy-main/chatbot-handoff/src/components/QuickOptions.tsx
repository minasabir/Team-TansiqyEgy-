import { Button } from '@/components/ui/button';
import {
  MessageCircle,
  GraduationCap,
  BookOpen,
  Globe,
  Landmark,
  Briefcase,
  Building2,
  Cpu,
  Library,
  School,
} from 'lucide-react';

interface QuickOption {
  id: string;
  label: string;
  icon: React.ReactNode;
  message: string;
}

interface QuickOptionsProps {
  options: QuickOption[];
  onSelect: (message: string) => void;
  disabled?: boolean;
  title?: string;
  showAskButton?: boolean;
}

export const universityTypeOptions: QuickOption[] = [
  {
    id: 'gov',
    label: 'جامعات حكومية',
    icon: <Landmark className="w-4 h-4" />,
    message: 'أنا مهتم بجامعات حكومية',
  },
  {
    id: 'private',
    label: 'جامعات خاصة',
    icon: <Briefcase className="w-4 h-4" />,
    message: 'أنا مهتم بجامعات خاصة',
  },
  {
    id: 'national',
    label: 'جامعات أهلية',
    icon: <Building2 className="w-4 h-4" />,
    message: 'أنا مهتم بجامعات أهلية',
  },
  {
    id: 'tech',
    label: 'جامعات تكنولوجية',
    icon: <Cpu className="w-4 h-4" />,
    message: 'أنا مهتم بجامعات تكنولوجية',
  },
  {
    id: 'foreign',
    label: 'جامعات أجنبية',
    icon: <Globe className="w-4 h-4" />,
    message: 'أنا مهتم بجامعات أجنبية',
  },
  {
    id: 'institutes',
    label: 'معاهد عليا',
    icon: <Library className="w-4 h-4" />,
    message: 'أنا مهتم بالمعاهد العليا',
  },
];

export const helpTypeOptions: QuickOption[] = [
  {
    id: 'colleges',
    label: 'عايز أعرف الكليات المتاحة لمجموعي',
    icon: <GraduationCap className="w-4 h-4" />,
    message: 'عايز أعرف الكليات المتاحة لمجموعي',
  },
  {
    id: 'compare',
    label: 'عايز أقارن بين كليتين',
    icon: <BookOpen className="w-4 h-4" />,
    message: 'عايز أقارن بين كليتين',
  },
  {
    id: 'jobs',
    label: 'عايز أعرف فرص الشغل',
    icon: <Globe className="w-4 h-4" />,
    message: 'عايز أعرف فرص الشغل لتخصص معين',
  },
  {
    id: 'tips',
    label: 'عايز نصائح للتنسيق',
    icon: <School className="w-4 h-4" />,
    message: 'إيه أهم النصائح للتنسيق؟',
  },
];

export function QuickOptions({ options, onSelect, disabled, title, showAskButton = true }: QuickOptionsProps) {
  return (
    <div className="flex flex-col gap-3 w-full" dir="rtl">
      {title && <p className="text-sm text-muted-foreground text-center mb-1">{title}</p>}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
        {options.map((option) => (
          <Button
            key={option.id}
            variant="outline"
            className="h-auto py-3 px-4 justify-start gap-3 text-right hover:bg-primary/10 hover:border-primary/50 transition-all duration-200"
            onClick={() => onSelect(option.message)}
            disabled={disabled}
          >
            <span className="flex-shrink-0 w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center">
              {option.icon}
            </span>
            <span className="text-sm leading-relaxed">{option.label}</span>
          </Button>
        ))}
      </div>
      {showAskButton && (
        <div className="mt-2">
          <Button
            variant="ghost"
            className="w-full h-auto py-3 px-4 justify-center gap-2 text-primary hover:bg-primary/5 border border-dashed border-primary/30 hover:border-primary/50 transition-all duration-200"
            onClick={() => onSelect('__ASK_CUSTOM__')}
            disabled={disabled}
          >
            <MessageCircle className="w-4 h-4" />
            <span className="text-sm">أريد أسأل سؤال</span>
          </Button>
        </div>
      )}
    </div>
  );
}
